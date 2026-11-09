using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

/// <summary>
/// Participant + event + category. Participants enrol; organisers read the start list for events they own.
/// </summary>
[Route("api")]
public class EnrolmentsController : RaceDayControllerBase
{
    public EnrolmentsController(RaceDayContext db) : base(db)
    {
    }

    /// <summary>
    /// Participant enters an event by choosing a category. The session supplies participantId. A bib number is assigned.
    /// </summary>
    [HttpPost("enrolments")]
    [SessionAuthorize(Roles.Participant)]
    [ProducesResponseType(typeof(EnrolmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enrol([FromBody] EnrolRequest request)
    {
        var ev = await Db.RaceEvents.FindAsync(request.EventId);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        var category = await Db.Categories.FindAsync(request.CategoryId);
        if (category is null)
        {
            return NotFound(new { message = "Category not found." });
        }

        if (category.EventId != request.EventId)
        {
            return BadRequest(new { message = "That category does not belong to this event." });
        }

        var already = await Db.Enrolments.AnyAsync(e =>
            e.EventId == request.EventId && e.ParticipantId == SessionUserId);
        if (already)
        {
            return ConflictMessage("You are already entered in this event.");
        }

        if (category.MaxParticipants is not null)
        {
            var taken = await Db.Enrolments.CountAsync(e =>
                e.CategoryId == category.CategoryId && e.Status != "Withdrawn");
            if (taken >= category.MaxParticipants)
            {
                return ConflictMessage("This category is full.");
            }
        }

        var existingBibs = await Db.Enrolments
            .Where(e => e.EventId == request.EventId && e.BibNumber != null)
            .Select(e => e.BibNumber!.Value)
            .ToListAsync();
        var nextBib = existingBibs.Count == 0 ? 100 : existingBibs.Max();

        var enrolment = new Enrolment
        {
            EventId = request.EventId,
            CategoryId = request.CategoryId,
            ParticipantId = SessionUserId,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Confirmed",
            BibNumber = nextBib + 1
        };
        Db.Enrolments.Add(enrolment);
        await Db.SaveChangesAsync();
        await Db.Entry(enrolment).Reference(e => e.Event).LoadAsync();
        await Db.Entry(enrolment).Reference(e => e.Category).LoadAsync();
        return Created($"/api/enrolments/{enrolment.EnrolmentId}", ToEnrolment(enrolment));
    }

    /// <summary>Participant lists their own entries and the events attached to them.</summary>
    [HttpGet("enrolments/me")]
    [SessionAuthorize(Roles.Participant)]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Mine()
    {
        var items = await Db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Where(e => e.ParticipantId == SessionUserId)
            .ToListAsync();
        return Ok(items.Select(ToEnrolment));
    }

    /// <summary>Organiser sees every enrolment on an event they own.</summary>
    [HttpGet("events/{id:int}/enrolments")]
    [SessionAuthorize(Roles.Organiser)]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ForEvent(int id)
    {
        var ev = await Db.RaceEvents.FindAsync(id);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (ev.OrganiserId != SessionUserId)
        {
            return Forbidden("You can only view enrolments for events you organised.");
        }

        var items = await Db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Where(e => e.EventId == id)
            .ToListAsync();
        return Ok(items.Select(ToEnrolment));
    }

    /// <summary>Participant may withdraw. Organiser may update bib number or status on their own event.</summary>
    [HttpPatch("enrolments/{id:int}")]
    [SessionAuthorize]
    [ProducesResponseType(typeof(EnrolmentResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchEnrolmentRequest request)
    {
        var enrolment = await Db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.EnrolmentId == id);

        if (enrolment is null)
        {
            return NotFound(new { message = "Enrolment not found." });
        }

        if (SessionRole == Roles.Participant)
        {
            if (enrolment.ParticipantId != SessionUserId)
            {
                return Forbidden("You can only change your own enrolment.");
            }

            if (!string.Equals(request.Status, "Withdrawn", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Participants may only withdraw (status = Withdrawn)." });
            }

            enrolment.Status = "Withdrawn";
        }
        else if (SessionRole == Roles.Organiser)
        {
            if (enrolment.Event!.OrganiserId != SessionUserId)
            {
                return Forbidden("You can only update enrolments on events you organised.");
            }

            if (request.Status is not null)
            {
                if (!EnrolmentStatuses.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
                {
                    return ConflictMessage("That status change is not allowed.");
                }

                enrolment.Status = EnrolmentStatuses.First(s =>
                    s.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
            }

            if (request.BibNumber is not null)
            {
                enrolment.BibNumber = request.BibNumber;
            }
        }
        else
        {
            return Forbidden("Unknown role.");
        }

        await Db.SaveChangesAsync();
        return Ok(ToEnrolment(enrolment));
    }
}
