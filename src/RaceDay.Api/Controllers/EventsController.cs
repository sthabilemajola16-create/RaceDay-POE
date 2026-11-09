using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

/// <summary>
/// Browse published events (public). Organisers create, update and delete events they own.
/// Each event stores name, description, date, location and type (run / walk / cycle).
/// Distance lives on categories.
/// </summary>
[Route("api/events")]
public class EventsController : RaceDayControllerBase
{
    public EventsController(RaceDayContext db) : base(db)
    {
    }

    /// <summary>Lists published events. Optional filters: sportType, province, from, to.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] string? sportType,
        [FromQuery] string? province,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        if (!string.IsNullOrWhiteSpace(sportType) && !SportTypes.Contains(sportType, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "sportType must be Running, Walking or Cycling." });
        }

        var query = Db.RaceEvents.AsQueryable();
        if (!string.IsNullOrWhiteSpace(sportType))
        {
            query = query.Where(e => e.SportType == sportType);
        }

        if (!string.IsNullOrWhiteSpace(province))
        {
            query = query.Where(e => e.Province == province);
        }

        if (from is not null)
        {
            query = query.Where(e => e.StartDateTime >= from);
        }

        if (to is not null)
        {
            query = query.Where(e => e.StartDateTime <= to);
        }

        var items = await query
            .OrderBy(e => e.StartDateTime)
            .Select(e => new EventListItemResponse
            {
                EventId = e.EventId,
                Name = e.Name,
                SportType = e.SportType,
                Venue = e.Venue,
                City = e.City,
                Province = e.Province,
                StartDateTime = e.StartDateTime,
                EndDateTime = e.EndDateTime,
                Status = e.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>Lists only events created by the signed-in organiser.</summary>
    [HttpGet("mine")]
    [SessionAuthorize(Roles.Organiser)]
    [ProducesResponseType(typeof(IEnumerable<EventListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Mine()
    {
        var items = await Db.RaceEvents
            .Where(e => e.OrganiserId == SessionUserId)
            .OrderBy(e => e.StartDateTime)
            .Select(e => new EventListItemResponse
            {
                EventId = e.EventId, Name = e.Name, SportType = e.SportType,
                Venue = e.Venue, City = e.City, Province = e.Province,
                StartDateTime = e.StartDateTime, EndDateTime = e.EndDateTime, Status = e.Status
            }).ToListAsync();
        return Ok(items);
    }

    /// <summary>One event with categories, route summary and the latest weather snapshot if one exists.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var ev = await Db.RaceEvents
            .Include(e => e.Categories)
            .Include(e => e.WeatherSnapshots)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        var latestWeather = ev.WeatherSnapshots.OrderByDescending(w => w.CapturedAt).FirstOrDefault();
        return Ok(new EventDetailResponse
        {
            EventId = ev.EventId,
            OrganiserId = ev.OrganiserId,
            Name = ev.Name,
            Description = ev.Description,
            SportType = ev.SportType,
            Venue = ev.Venue,
            City = ev.City,
            Province = ev.Province,
            StartDateTime = ev.StartDateTime,
            EndDateTime = ev.EndDateTime,
            Status = ev.Status,
            RouteSummary = ev.RouteSummary,
            Categories = ev.Categories.Select(ToCategory).ToList(),
            LatestWeather = latestWeather is null
                ? null
                : new WeatherSnapshotResponse
                {
                    WeatherSnapshotId = latestWeather.WeatherSnapshotId,
                    EventId = latestWeather.EventId,
                    CapturedAt = latestWeather.CapturedAt,
                    TemperatureC = latestWeather.TemperatureC,
                    Condition = latestWeather.Condition,
                    WindKph = latestWeather.WindKph,
                    HumidityPercent = latestWeather.HumidityPercent,
                    Source = latestWeather.Source
                }
        });
    }

    /// <summary>Organiser creates an event. OrganiserId is taken from the session, not from the body.</summary>
    [HttpPost]
    [SessionAuthorize(Roles.Organiser)]
    [ProducesResponseType(typeof(EventDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] EventWriteRequest request)
    {
        var error = ValidateEvent(request);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        var ev = new RaceEvent
        {
            OrganiserId = SessionUserId,
            CreatedAt = DateTime.UtcNow
        };
        Apply(ev, request);
        Db.RaceEvents.Add(ev);
        await Db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = ev.EventId }, await Detail(ev.EventId));
    }

    /// <summary>Organiser updates an event they own.</summary>
    [HttpPut("{id:int}")]
    [SessionAuthorize(Roles.Organiser)]
    [ProducesResponseType(typeof(EventDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] EventWriteRequest request)
    {
        var ev = await Db.RaceEvents.FindAsync(id);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (ev.OrganiserId != SessionUserId)
        {
            return Forbidden("You can only update events you organised.");
        }

        var error = ValidateEvent(request);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        Apply(ev, request);
        await Db.SaveChangesAsync();
        return Ok(await Detail(ev.EventId));
    }

    /// <summary>Organiser deletes an event they own, unless confirmed enrolments already exist.</summary>
    [HttpDelete("{id:int}")]
    [SessionAuthorize(Roles.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await Db.RaceEvents.FindAsync(id);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (ev.OrganiserId != SessionUserId)
        {
            return Forbidden("You can only delete events you organised.");
        }

        var blocked = await Db.Enrolments.AnyAsync(e => e.EventId == id && e.Status == "Confirmed");
        if (blocked)
        {
            return ConflictMessage("This event still has confirmed enrolments.");
        }

        Db.RaceEvents.Remove(ev);
        await Db.SaveChangesAsync();
        return NoContent();
    }

    private string? ValidateEvent(EventWriteRequest request)
    {
        if (!SportTypes.Contains(request.SportType, StringComparer.OrdinalIgnoreCase))
        {
            return "sportType must be Running, Walking or Cycling.";
        }

        if (!EventStatuses.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
        {
            return "status must be Draft, Published, Closed or Completed.";
        }

        if (request.EndDateTime <= request.StartDateTime)
        {
            return "endDateTime must be after startDateTime.";
        }

        return null;
    }

    private static void Apply(RaceEvent ev, EventWriteRequest request)
    {
        ev.Name = request.Name.Trim();
        ev.Description = request.Description;
        ev.SportType = SportTypes.First(s => s.Equals(request.SportType, StringComparison.OrdinalIgnoreCase));
        ev.Venue = request.Venue.Trim();
        ev.City = request.City.Trim();
        ev.Province = request.Province.Trim();
        ev.StartDateTime = request.StartDateTime;
        ev.EndDateTime = request.EndDateTime;
        ev.Status = EventStatuses.First(s => s.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
        ev.RouteSummary = request.RouteSummary;
    }

    private async Task<EventDetailResponse?> Detail(int id)
    {
        var result = await Get(id);
        return (result as OkObjectResult)?.Value as EventDetailResponse;
    }
}
