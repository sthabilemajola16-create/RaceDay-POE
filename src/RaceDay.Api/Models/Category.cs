namespace RaceDay.Api.Models;

public class Category
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public decimal EntryFeeZar { get; set; }
    public int? MaxParticipants { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    public RaceEvent? Event { get; set; }
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
