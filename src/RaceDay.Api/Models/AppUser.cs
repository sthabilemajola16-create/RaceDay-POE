namespace RaceDay.Api.Models;

public class AppUser
{
    public int UserId { get; set; }
    public int? ClubId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }

    public Club? Club { get; set; }
    public ICollection<RaceEvent> OrganisedEvents { get; set; } = new List<RaceEvent>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
