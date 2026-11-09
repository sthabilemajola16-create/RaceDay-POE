using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

/// <summary>Cached weather snapshots for race-day preparation.</summary>
[Route("api/events/{eventId:int}/weather")]
public class WeatherController : RaceDayControllerBase
{
    public WeatherController(RaceDayContext db) : base(db)
    {
    }

    /// <summary>Latest stored weather snapshot for the event.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(WeatherSnapshotResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Latest(int eventId)
    {
        if (!await Db.RaceEvents.AnyAsync(e => e.EventId == eventId))
        {
            return NotFound(new { message = "Event not found." });
        }

        var snap = await Db.WeatherSnapshots
            .Where(w => w.EventId == eventId)
            .OrderByDescending(w => w.CapturedAt)
            .FirstOrDefaultAsync();

        if (snap is null)
        {
            return NotFound(new { message = "No weather snapshot has been stored for this event yet." });
        }

        return Ok(ToWeather(snap));
    }

    /// <summary>Organiser stores a weather snapshot (temperature, condition, wind, humidity, source).</summary>
    [HttpPost]
    [SessionAuthorize(Roles.Organiser)]
    [ProducesResponseType(typeof(WeatherSnapshotResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(int eventId, [FromBody] WeatherWriteRequest request)
    {
        var ev = await Db.RaceEvents.FindAsync(eventId);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (ev.OrganiserId != SessionUserId)
        {
            return Forbidden("You can only store weather on events you organised.");
        }

        if (request.HumidityPercent is < 0 or > 100)
        {
            return BadRequest(new { message = "humidityPercent must be between 0 and 100." });
        }

        var snap = new WeatherSnapshot
        {
            EventId = eventId,
            CapturedAt = DateTime.UtcNow,
            TemperatureC = request.TemperatureC,
            Condition = request.Condition.Trim(),
            WindKph = request.WindKph,
            HumidityPercent = request.HumidityPercent,
            Source = string.IsNullOrWhiteSpace(request.Source) ? "OpenWeather" : request.Source
        };
        Db.WeatherSnapshots.Add(snap);
        await Db.SaveChangesAsync();
        return Created($"/api/events/{eventId}/weather", ToWeather(snap));
    }

    private static WeatherSnapshotResponse ToWeather(WeatherSnapshot snap) => new()
    {
        WeatherSnapshotId = snap.WeatherSnapshotId,
        EventId = snap.EventId,
        CapturedAt = snap.CapturedAt,
        TemperatureC = snap.TemperatureC,
        Condition = snap.Condition,
        WindKph = snap.WindKph,
        HumidityPercent = snap.HumidityPercent,
        Source = snap.Source
    };
}
