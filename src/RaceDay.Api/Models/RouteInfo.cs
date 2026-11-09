namespace RaceDay.Api.Models;

public class RouteInfo
{
    public int RouteId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int? ElevationGainM { get; set; }
    public string? MapNotes { get; set; }
    public string? GpxUrl { get; set; }

    public RaceEvent? Event { get; set; }
}
