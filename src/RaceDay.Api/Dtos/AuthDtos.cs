using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

/// <summary>Body for POST /api/auth/register.</summary>
public class RegisterRequest
{
    /// <example>lerato.new@email.com</example>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <example>Password1!</example>
    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>Must be Organiser or Participant.</summary>
    /// <example>Participant</example>
    [Required]
    public string Role { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public int? ClubId { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}

/// <summary>Body for POST /api/auth/login.</summary>
public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class SessionResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class AuthUserResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int? ClubId { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}
