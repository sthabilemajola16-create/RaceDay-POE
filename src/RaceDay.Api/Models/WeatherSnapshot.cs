namespace RaceDay.Api.Models;

public class WeatherSnapshot
{
    public int WeatherSnapshotId { get; set; }
    public int EventId { get; set; }
    public DateTime CapturedAt { get; set; }
    public decimal TemperatureC { get; set; }
    public string Condition { get; set; } = string.Empty;
    public decimal? WindKph { get; set; }
    public int? HumidityPercent { get; set; }
    public string Source { get; set; } = "OpenWeather";

    public RaceEvent? Event { get; set; }
}
