namespace RaceDay.Api.Models;

public class RaceEvent
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SportType { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Status { get; set; } = "Published";
    public string? RouteSummary { get; set; }
    public DateTime CreatedAt { get; set; }

    public AppUser? Organiser { get; set; }
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<RouteInfo> Routes { get; set; } = new List<RouteInfo>();
    public ICollection<WeatherSnapshot> WeatherSnapshots { get; set; } = new List<WeatherSnapshot>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
