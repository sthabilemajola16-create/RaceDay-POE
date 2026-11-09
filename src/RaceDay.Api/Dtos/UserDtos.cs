using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class UpdateProfileRequest
{
    [Required, MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public int? ClubId { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}

public class PublicProfileResponse
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? ClubId { get; set; }
}
