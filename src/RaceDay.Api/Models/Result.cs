namespace RaceDay.Api.Models;

public class Result
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? PositionOverall { get; set; }
    public string ResultStatus { get; set; } = "Finished";
    public int CapturedByOrganiserId { get; set; }
    public DateTime CapturedAt { get; set; }

    public Enrolment? Enrolment { get; set; }
    public AppUser? CapturedByOrganiser { get; set; }
}
