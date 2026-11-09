using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests;

public class EnrolmentTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;

    public EnrolmentTests(RaceDayApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Participant_can_enrol_and_row_stores_participant_event_and_category()
    {
        var client = _factory.CreateClient();
        await Login(client, TestData.ParticipantEmail);

        var events = await client.GetFromJsonAsync<List<EventListDto>>("/api/events");
        var eventId = events!.First().EventId;
        var categories = await client.GetFromJsonAsync<List<CategoryDto>>($"/api/events/{eventId}/categories");
        var categoryId = categories!.First().CategoryId;

        var response = await client.PostAsJsonAsync("/api/enrolments", new { eventId, categoryId });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var enrolment = await response.Content.ReadFromJsonAsync<EnrolmentDto>();
        Assert.Equal(eventId, enrolment!.EventId);
        Assert.Equal(categoryId, enrolment.CategoryId);
        Assert.True(enrolment.BibNumber is > 0);
        Assert.Equal("Confirmed", enrolment.Status);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
        var row = db.Enrolments.Single(e => e.EnrolmentId == enrolment.EnrolmentId);
        Assert.Equal(eventId, row.EventId);
        Assert.Equal(categoryId, row.CategoryId);
        Assert.Equal(enrolment.ParticipantId, row.ParticipantId);
        Assert.True(row.ParticipantId > 0);
    }

    [Fact]
    public async Task Second_enrolment_on_same_event_returns_409()
    {
        var client = _factory.CreateClient();
        var email = $"runner.{Guid.NewGuid():N}@email.com";
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = TestData.Password,
            firstName = "Palesa",
            lastName = "Molefe",
            role = "Participant"
        });
        await Login(client, email);

        var events = await client.GetFromJsonAsync<List<EventListDto>>("/api/events");
        var eventId = events!.First().EventId;
        var categories = await client.GetFromJsonAsync<List<CategoryDto>>($"/api/events/{eventId}/categories");
        var categoryId = categories!.First().CategoryId;

        var first = await client.PostAsJsonAsync("/api/enrolments", new { eventId, categoryId });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/enrolments", new { eventId, categoryId });
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Category_from_another_event_returns_400()
    {
        var org = _factory.CreateClient();
        await Login(org, TestData.OrganiserEmail);
        var created = await org.PostAsJsonAsync("/api/events", new
        {
            name = "Second Event",
            description = "Other event",
            sportType = "Running",
            venue = "Beach",
            city = "Durban",
            province = "KwaZulu-Natal",
            startDateTime = "2026-10-11T06:00:00",
            endDateTime = "2026-10-11T12:00:00",
            status = "Published"
        });
        var otherEvent = await created.Content.ReadFromJsonAsync<EventListDto>();
        var addCat = await org.PostAsJsonAsync($"/api/events/{otherEvent!.EventId}/categories", new
        {
            name = "21.1 km",
            distanceKm = 21.1,
            entryFeeZar = 280
        });
        var otherCategory = await addCat.Content.ReadFromJsonAsync<CategoryDto>();

        var participant = _factory.CreateClient();
        await Login(participant, TestData.ParticipantEmail);
        var seedEvents = await participant.GetFromJsonAsync<List<EventListDto>>("/api/events");
        var seedEventId = seedEvents!.First(e => e.EventId != otherEvent.EventId).EventId;

        var response = await participant.PostAsJsonAsync("/api/enrolments", new
        {
            eventId = seedEventId,
            categoryId = otherCategory!.CategoryId
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_enrol_returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/enrolments", new { eventId = 1, categoryId = 1 });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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

    private sealed class EventListDto
    {
        public int EventId { get; set; }
    }

    private sealed class CategoryDto
    {
        public int CategoryId { get; set; }
    }

    private sealed class EnrolmentDto
    {
        public int EnrolmentId { get; set; }
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public int ParticipantId { get; set; }
        public int? BibNumber { get; set; }
        public string Status { get; set; } = "";
    }
}
