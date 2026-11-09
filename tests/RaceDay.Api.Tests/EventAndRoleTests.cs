using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

public class EventAndRoleTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;

    public EventAndRoleTests(RaceDayApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Organiser_can_create_update_and_delete_own_event()
    {
        var client = _factory.CreateClient();
        await Login(client, TestData.OrganiserEmail);

        var create = await client.PostAsJsonAsync("/api/events", NewEvent("Alexandra Night 5 km"));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<EventDto>();
        Assert.Equal("Walking", created!.SportType);
        Assert.True(created.EventId > 0);

        var update = await client.PutAsJsonAsync($"/api/events/{created.EventId}", NewEvent("Alexandra Night 8 km"));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var delete = await client.DeleteAsync($"/api/events/{created.EventId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact]
    public async Task Participant_cannot_create_event()
    {
        var client = _factory.CreateClient();
        await Login(client, TestData.ParticipantEmail);

        var response = await client.PostAsJsonAsync("/api/events", NewEvent("Illegal Event"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_cannot_edit_someone_elses_event()
    {
        var ownerClient = _factory.CreateClient();
        await Login(ownerClient, TestData.OrganiserEmail);
        var create = await ownerClient.PostAsJsonAsync("/api/events", NewEvent("Owner Only Event"));
        var created = await create.Content.ReadFromJsonAsync<EventDto>();

        var otherClient = _factory.CreateClient();
        await Login(otherClient, TestData.OtherOrganiserEmail);
        var update = await otherClient.PutAsJsonAsync($"/api/events/{created!.EventId}", NewEvent("Hijacked Name"));
        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
    }

    [Fact]
    public async Task Participant_cannot_capture_results()
    {
        var client = _factory.CreateClient();
        await Login(client, TestData.ParticipantEmail);
        var response = await client.PostAsJsonAsync("/api/results", new
        {
            enrolmentId = 1,
            positionOverall = 12,
            resultStatus = "Finished"
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_cannot_enrol_as_a_participant()
    {
        var client = _factory.CreateClient();
        await Login(client, TestData.OrganiserEmail);
        var response = await client.PostAsJsonAsync("/api/enrolments", new { eventId = 1, categoryId = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_users_can_browse_events()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task Login(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = TestData.Password
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static object NewEvent(string name) => new
    {
        name,
        description = "Unit-test event",
        sportType = "Walking",
        venue = "Test Grounds",
        city = "Johannesburg",
        province = "Gauteng",
        startDateTime = "2026-12-01T06:30:00",
        endDateTime = "2026-12-01T10:00:00",
        status = "Published",
        routeSummary = "Flat park loop"
    };

    private sealed class EventDto
    {
        public int EventId { get; set; }
        public string SportType { get; set; } = "";
    }
}
