using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Controllers;

/// <summary>Own profile for both roles, plus organiser lookup of a participant.</summary>
[Route("api/users")]
public class UsersController : RaceDayControllerBase
{
    public UsersController(RaceDayContext db) : base(db)
    {
    }

    /// <summary>Returns the authenticated user's profile (no password hash).</summary>
    [HttpGet("me")]
    [SessionAuthorize]
    [ProducesResponseType(typeof(AuthUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        var user = await Db.AppUsers.FindAsync(SessionUserId);
        if (user is null)
        {
            return NotFound(new { message = "User row is missing." });
        }

        return Ok(ToProfile(user));
    }

    /// <summary>
    /// Updates name, phone, club and date of birth. Role and email stay as they were at registration.
    /// </summary>
    [HttpPut("me")]
    [SessionAuthorize]
    [ProducesResponseType(typeof(AuthUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var user = await Db.AppUsers.FindAsync(SessionUserId);
        if (user is null)
        {
            return NotFound(new { message = "User row is missing." });
        }

        if (request.ClubId is not null && !await Db.Clubs.AnyAsync(c => c.ClubId == request.ClubId))
        {
            return BadRequest(new { message = "Unknown clubId." });
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Phone = request.Phone;
        user.ClubId = request.ClubId;
        user.DateOfBirth = request.DateOfBirth;
        await Db.SaveChangesAsync();
        return Ok(ToProfile(user));
    }

    /// <summary>Organiser reads a participant's public fields when capturing results or checking an entry list.</summary>
    [HttpGet("{id:int}")]
    [SessionAuthorize(Roles.Organiser)]
    [ProducesResponseType(typeof(PublicProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await Db.AppUsers.FindAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(new PublicProfileResponse
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            ClubId = user.ClubId
        });
    }
}
