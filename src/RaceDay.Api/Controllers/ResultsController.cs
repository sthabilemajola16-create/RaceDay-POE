using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

/// <summary>Finish times and positions. Organisers capture them; participants read their own history.</summary>
[Route("api")]
public class ResultsController : RaceDayControllerBase
{
    public ResultsController(RaceDayContext db) : base(db)
    {
    }

    /// <summary>Organiser captures a result for one enrolment on an event they own.</summary>
    [HttpPost("results")]
    [SessionAuthorize(Roles.Organiser)]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] ResultWriteRequest request)
    {
        if (request.EnrolmentId is null)
        {
            return BadRequest(new { message = "enrolmentId is required." });
        }

        if (!ResultStatuses.Contains(request.ResultStatus, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "resultStatus must be Finished, DNF, DSQ or DNS." });
        }

        var enrolment = await Db.Enrolments.Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentId == request.EnrolmentId);
        if (enrolment is null)
        {
            return NotFound(new { message = "Enrolment not found." });
        }

        if (enrolment.Event!.OrganiserId != SessionUserId)
        {
            return Forbidden("You can only capture results on events you organised.");
        }

        if (await Db.Results.AnyAsync(r => r.EnrolmentId == request.EnrolmentId))
        {
            return ConflictMessage("A result already exists for this enrolment.");
        }

        var result = new Result
        {
            EnrolmentId = request.EnrolmentId.Value,
            FinishTime = request.FinishTime,
            PositionOverall = request.PositionOverall,
            ResultStatus = ResultStatuses.First(s => s.Equals(request.ResultStatus, StringComparison.OrdinalIgnoreCase)),
            CapturedByOrganiserId = SessionUserId,
            CapturedAt = DateTime.UtcNow
        };
        Db.Results.Add(result);
        await Db.SaveChangesAsync();
        return Created($"/api/results/{result.ResultId}", ToResult(result));
    }

    /// <summary>Organiser corrects a captured result.</summary>
    [HttpPut("results/{id:int}")]
    [SessionAuthorize(Roles.Organiser)]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] ResultWriteRequest request)
    {
        var result = await Db.Results
            .Include(r => r.Enrolment)!.ThenInclude(e => e!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);
        if (result is null)
        {
            return NotFound(new { message = "Result not found." });
        }

        if (result.Enrolment!.Event!.OrganiserId != SessionUserId)
        {
            return Forbidden("You can only edit results on events you organised.");
        }

        if (!ResultStatuses.Contains(request.ResultStatus, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "resultStatus must be Finished, DNF, DSQ or DNS." });
        }

        result.FinishTime = request.FinishTime;
        result.PositionOverall = request.PositionOverall;
        result.ResultStatus = ResultStatuses.First(s => s.Equals(request.ResultStatus, StringComparison.OrdinalIgnoreCase));
        await Db.SaveChangesAsync();
        return Ok(ToResult(result));
    }

    /// <summary>Participant reads their own result history.</summary>
    [HttpGet("results/me")]
    [SessionAuthorize(Roles.Participant)]
    [ProducesResponseType(typeof(IEnumerable<ResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Mine()
    {
        var items = await Db.Results
            .Include(r => r.Enrolment)
            .Where(r => r.Enrolment!.ParticipantId == SessionUserId)
            .ToListAsync();
        return Ok(items.Select(ToResult));
    }

    /// <summary>Published results list for an event (public).</summary>
    [HttpGet("events/{id:int}/results")]
    [ProducesResponseType(typeof(IEnumerable<ResultResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForEvent(int id)
    {
        if (!await Db.RaceEvents.AnyAsync(e => e.EventId == id))
        {
            return NotFound(new { message = "Event not found." });
        }

        var items = await Db.Results
            .Include(r => r.Enrolment)
            .Where(r => r.Enrolment!.EventId == id)
            .OrderBy(r => r.PositionOverall)
            .ToListAsync();
        return Ok(items.Select(ToResult));
    }
}
