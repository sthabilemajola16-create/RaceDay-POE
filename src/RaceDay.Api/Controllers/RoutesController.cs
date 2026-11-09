using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

/// <summary>Route notes and GPX URLs for race-day preparation (Part 1 ERD table Route).</summary>
[Route("api/events/{eventId:int}/routes")]
public class RoutesController : RaceDayControllerBase
{
    public RoutesController(RaceDayContext db) : base(db)
    {
    }

    /// <summary>Returns route notes and GPX / blob URLs for an event.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RouteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(int eventId)
    {
        if (!await Db.RaceEvents.AnyAsync(e => e.EventId == eventId))
        {
            return NotFound(new { message = "Event not found." });
        }

        var items = await Db.Routes.Where(r => r.EventId == eventId).ToListAsync();
        return Ok(items.Select(ToRoute));
    }

    /// <summary>Organiser adds a route record. Part 3 can store an Azure Blob GPX URL in gpxUrl.</summary>
    [HttpPost]
    [SessionAuthorize(Roles.Organiser)]
    [ProducesResponseType(typeof(RouteResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(int eventId, [FromBody] RouteWriteRequest request)
    {
        var ev = await Db.RaceEvents.FindAsync(eventId);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (ev.OrganiserId != SessionUserId)
        {
            return Forbidden("You can only add routes to events you organised.");
        }

        if (request.DistanceKm <= 0)
        {
            return BadRequest(new { message = "distanceKm must be greater than zero." });
        }

        var route = new RouteInfo
        {
            EventId = eventId,
            Name = request.Name.Trim(),
            DistanceKm = request.DistanceKm,
            ElevationGainM = request.ElevationGainM,
            MapNotes = request.MapNotes,
            GpxUrl = request.GpxUrl
        };
        Db.Routes.Add(route);
        await Db.SaveChangesAsync();
        return Created($"/api/events/{eventId}/routes", ToRoute(route));
    }

    private static RouteResponse ToRoute(RouteInfo route) => new()
    {
        RouteId = route.RouteId,
        EventId = route.EventId,
        Name = route.Name,
        DistanceKm = route.DistanceKm,
        ElevationGainM = route.ElevationGainM,
        MapNotes = route.MapNotes,
        GpxUrl = route.GpxUrl
    };
}
