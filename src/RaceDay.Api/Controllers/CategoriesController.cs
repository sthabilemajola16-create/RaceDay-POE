using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

/// <summary>Age or distance categories (Under 20, Senior, 10 km, 21 km) attached to an event.</summary>
[Route("api")]
public class CategoriesController : RaceDayControllerBase
{
    public CategoriesController(RaceDayContext db) : base(db)
    {
    }

    /// <summary>Lists categories for one event. Both roles (and the public) can view them.</summary>
    [HttpGet("events/{eventId:int}/categories")]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(int eventId)
    {
        if (!await Db.RaceEvents.AnyAsync(e => e.EventId == eventId))
        {
            return NotFound(new { message = "Event not found." });
        }

        var items = await Db.Categories.Where(c => c.EventId == eventId).ToListAsync();
        return Ok(items.Select(ToCategory));
    }

    /// <summary>Organiser adds a category to an event they own.</summary>
    [HttpPost("events/{eventId:int}/categories")]
    [SessionAuthorize(Roles.Organiser)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(int eventId, [FromBody] CategoryWriteRequest request)
    {
        var ev = await Db.RaceEvents.FindAsync(eventId);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (ev.OrganiserId != SessionUserId)
        {
            return Forbidden("You can only add categories to events you organised.");
        }

        var error = ValidateCategory(request);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        var category = new Category { EventId = eventId };
        Apply(category, request);
        Db.Categories.Add(category);
        await Db.SaveChangesAsync();
        return Created($"/api/categories/{category.CategoryId}", ToCategory(category));
    }

    /// <summary>Organiser updates a category on an event they own.</summary>
    [HttpPut("categories/{id:int}")]
    [SessionAuthorize(Roles.Organiser)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryWriteRequest request)
    {
        var category = await Db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category is null)
        {
            return NotFound(new { message = "Category not found." });
        }

        if (category.Event!.OrganiserId != SessionUserId)
        {
            return Forbidden("You can only update categories on events you organised.");
        }

        var error = ValidateCategory(request);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        Apply(category, request);
        await Db.SaveChangesAsync();
        return Ok(ToCategory(category));
    }

    /// <summary>Organiser removes a category that nobody has enrolled into yet.</summary>
    [HttpDelete("categories/{id:int}")]
    [SessionAuthorize(Roles.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await Db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category is null)
        {
            return NotFound(new { message = "Category not found." });
        }

        if (category.Event!.OrganiserId != SessionUserId)
        {
            return Forbidden("You can only delete categories on events you organised.");
        }

        if (await Db.Enrolments.AnyAsync(e => e.CategoryId == id))
        {
            return ConflictMessage("This category already has enrolments.");
        }

        Db.Categories.Remove(category);
        await Db.SaveChangesAsync();
        return NoContent();
    }

    private static string? ValidateCategory(CategoryWriteRequest request)
    {
        if (request.MinAge is not null && request.MaxAge is not null && request.MaxAge < request.MinAge)
        {
            return "maxAge cannot be below minAge.";
        }

        return null;
    }

    private static void Apply(Category category, CategoryWriteRequest request)
    {
        category.Name = request.Name.Trim();
        category.DistanceKm = request.DistanceKm;
        category.EntryFeeZar = request.EntryFeeZar;
        category.MaxParticipants = request.MaxParticipants;
        category.MinAge = request.MinAge;
        category.MaxAge = request.MaxAge;
    }
}
