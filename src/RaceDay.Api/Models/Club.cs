namespace RaceDay.Api.Models;

public class Club
{
    public int ClubId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int? FoundedYear { get; set; }

    public ICollection<AppUser> Members { get; set; } = new List<AppUser>();
}
