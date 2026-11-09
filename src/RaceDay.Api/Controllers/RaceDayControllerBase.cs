using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
public abstract class RaceDayControllerBase : ControllerBase
{
    protected readonly RaceDayContext Db;

    protected RaceDayControllerBase(RaceDayContext db)
    {
        Db = db;
    }

    protected int SessionUserId => HttpContext.Session.GetInt32(SessionKeys.UserId)
        ?? throw new InvalidOperationException("Session has no user id.");

    protected string SessionRole => HttpContext.Session.GetString(SessionKeys.Role) ?? string.Empty;

    protected static readonly string[] SportTypes = { "Running", "Walking", "Cycling" };
    protected static readonly string[] EventStatuses = { "Draft", "Published", "Closed", "Completed" };
    protected static readonly string[] EnrolmentStatuses = { "Pending", "Confirmed", "Withdrawn", "DNS", "DNF" };
    protected static readonly string[] ResultStatuses = { "Finished", "DNF", "DSQ", "DNS" };

    protected ObjectResult Forbidden(string message) =>
        StatusCode(StatusCodes.Status403Forbidden, new { message });

    protected ObjectResult ConflictMessage(string message) =>
        Conflict(new { message });

    protected static AuthUserResponse ToProfile(AppUser user) => new()
    {
        UserId = user.UserId,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Role = user.Role,
        Phone = user.Phone,
        ClubId = user.ClubId,
        DateOfBirth = user.DateOfBirth
    };

    protected static CategoryResponse ToCategory(Category category) => new()
    {
        CategoryId = category.CategoryId,
        EventId = category.EventId,
        Name = category.Name,
        DistanceKm = category.DistanceKm,
        EntryFeeZar = category.EntryFeeZar,
        MaxParticipants = category.MaxParticipants,
        MinAge = category.MinAge,
        MaxAge = category.MaxAge
    };

    protected static EnrolmentResponse ToEnrolment(Enrolment enrolment) => new()
    {
        EnrolmentId = enrolment.EnrolmentId,
        EventId = enrolment.EventId,
        CategoryId = enrolment.CategoryId,
        ParticipantId = enrolment.ParticipantId,
        EnrolmentDate = enrolment.EnrolmentDate,
        Status = enrolment.Status,
        BibNumber = enrolment.BibNumber,
        EventName = enrolment.Event?.Name,
        CategoryName = enrolment.Category?.Name
    };

    protected static ResultResponse ToResult(Result result) => new()
    {
        ResultId = result.ResultId,
        EnrolmentId = result.EnrolmentId,
        FinishTime = result.FinishTime,
        PositionOverall = result.PositionOverall,
        ResultStatus = result.ResultStatus,
        CapturedAt = result.CapturedAt
    };
}
