namespace RaceDay.Api.Models;

public class Enrolment
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public int ParticipantId { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = "Confirmed";
    public int? BibNumber { get; set; }

    public RaceEvent? Event { get; set; }
    public Category? Category { get; set; }
    public AppUser? Participant { get; set; }
    public Result? Result { get; set; }
}
