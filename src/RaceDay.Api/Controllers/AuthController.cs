using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

/// <summary>
/// Register, log in, inspect the server-side session, and log out.
/// Login stores user ID and role in the session cookie used by every later request.
/// </summary>
[Route("api/auth")]
public class AuthController : RaceDayControllerBase
{
    private readonly IPasswordHasher<AppUser> _hasher;

    public AuthController(RaceDayContext db, IPasswordHasher<AppUser> hasher) : base(db)
    {
        _hasher = hasher;
    }

    /// <summary>
    /// Creates a RaceDay account as Organiser or Participant. The password is hashed before it is saved.
    /// </summary>
    /// <response code="201">Profile created (password is never returned).</response>
    /// <response code="400">Missing fields or role is not Organiser/Participant.</response>
    /// <response code="409">That email is already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!Roles.IsKnown(request.Role))
        {
            return BadRequest(new { message = "Role must be Organiser or Participant." });
        }

        if (request.ClubId is not null && !await Db.Clubs.AnyAsync(c => c.ClubId == request.ClubId))
        {
            return BadRequest(new { message = "Unknown clubId." });
        }

        var email = request.Email.Trim();
        if (await Db.AppUsers.AnyAsync(u => u.Email == email))
        {
            return ConflictMessage("An account with that email already exists.");
        }

        var user = new AppUser
        {
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = string.Equals(request.Role, Roles.Organiser, StringComparison.OrdinalIgnoreCase)
                ? Roles.Organiser
                : Roles.Participant,
            Phone = request.Phone,
            ClubId = request.ClubId,
            DateOfBirth = request.DateOfBirth,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = "pending"
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        Db.AppUsers.Add(user);
        await Db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, ToProfile(user));
    }

    /// <summary>
    /// Checks email and password, then writes user ID and role into a server-side session (HTTP-only cookie).
    /// </summary>
    /// <response code="200">Logged in. Swagger will send the cookie on the next call.</response>
    /// <response code="401">Unknown email or wrong password.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await Db.AppUsers.FirstOrDefaultAsync(u => u.Email == request.Email.Trim());
        if (user is null)
        {
            return Unauthorized(new { message = "Unknown email or wrong password." });
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Unknown email or wrong password." });
        }

        HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserId);
        HttpContext.Session.SetString(SessionKeys.Role, user.Role);
        HttpContext.Session.SetString(SessionKeys.Email, user.Email);
        await HttpContext.Session.CommitAsync();

        return Ok(ToProfile(user));
    }

    /// <summary>
    /// Ends the session so the cookie can no longer call protected routes.
    /// </summary>
    [HttpPost("logout")]
    [SessionAuthorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.Session.CommitAsync();
        return NoContent();
    }

    /// <summary>
    /// Returns the current session (user ID, role, email) so you can see what the server stored on login.
    /// </summary>
    [HttpGet("session")]
    [SessionAuthorize]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Session()
    {
        return Ok(new SessionResponse
        {
            UserId = SessionUserId,
            Email = HttpContext.Session.GetString(SessionKeys.Email) ?? string.Empty,
            Role = SessionRole
        });
    }
}
