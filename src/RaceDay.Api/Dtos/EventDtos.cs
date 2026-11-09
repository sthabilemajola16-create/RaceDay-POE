using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class EventWriteRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Running, Walking or Cycling.</summary>
    [Required]
    public string SportType { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Venue { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Province { get; set; } = string.Empty;

    [Required]
    public DateTime StartDateTime { get; set; }

    [Required]
    public DateTime EndDateTime { get; set; }

    public string Status { get; set; } = "Published";
    public string? RouteSummary { get; set; }
}

public class EventListItemResponse
{
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SportType { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class EventDetailResponse : EventListItemResponse
{
    public int OrganiserId { get; set; }
    public string? Description { get; set; }
    public string? RouteSummary { get; set; }
    public List<CategoryResponse> Categories { get; set; } = new();
    public WeatherSnapshotResponse? LatestWeather { get; set; }
}

public class CategoryWriteRequest
{
    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 1000)]
    public decimal DistanceKm { get; set; }

    [Range(0, 100000)]
    public decimal EntryFeeZar { get; set; }

    public int? MaxParticipants { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
}

public class CategoryResponse
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public decimal EntryFeeZar { get; set; }
    public int? MaxParticipants { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
}

public class EnrolRequest
{
    [Required]
    public int EventId { get; set; }

    [Required]
    public int CategoryId { get; set; }
}

public class EnrolmentResponse
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public int ParticipantId { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? BibNumber { get; set; }
    public string? EventName { get; set; }
    public string? CategoryName { get; set; }
}

public class PatchEnrolmentRequest
{
    public string? Status { get; set; }
    public int? BibNumber { get; set; }
}

public class ResultWriteRequest
{
    public int? EnrolmentId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? PositionOverall { get; set; }

    [Required]
    public string ResultStatus { get; set; } = "Finished";
}

public class ResultResponse
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? PositionOverall { get; set; }
    public string ResultStatus { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; }
}

public class RouteWriteRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public decimal DistanceKm { get; set; }
    public int? ElevationGainM { get; set; }
    public string? MapNotes { get; set; }
    public string? GpxUrl { get; set; }
}

public class RouteResponse
{
    public int RouteId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int? ElevationGainM { get; set; }
    public string? MapNotes { get; set; }
    public string? GpxUrl { get; set; }
}

public class WeatherWriteRequest
{
    public decimal TemperatureC { get; set; }
    [Required]
    public string Condition { get; set; } = string.Empty;
    public decimal? WindKph { get; set; }
    public int? HumidityPercent { get; set; }
    public string Source { get; set; } = "OpenWeather";
}

public class WeatherSnapshotResponse
{
    public int WeatherSnapshotId { get; set; }
    public int EventId { get; set; }
    public DateTime CapturedAt { get; set; }
    public decimal TemperatureC { get; set; }
    public string Condition { get; set; } = string.Empty;
    public decimal? WindKph { get; set; }
    public int? HumidityPercent { get; set; }
    public string Source { get; set; } = string.Empty;
}

public class ClubResponse
{
    public int ClubId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int? FoundedYear { get; set; }
}
