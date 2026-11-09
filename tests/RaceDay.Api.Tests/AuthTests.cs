using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests;

public class AuthTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;

    public AuthTests(RaceDayApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_organiser_hashes_password_and_returns_201()
    {
        var client = _factory.CreateClient();
        var email = $"org.{Guid.NewGuid():N}@raceday.co.za";

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = TestData.Password,
            firstName = "Sipho",
            lastName = "Dlamini",
            role = "Organiser"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthUserDto>();
        Assert.Equal("Organiser", body!.Role);
        Assert.Equal(email, body.Email);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
        var stored = db.AppUsers.Single(u => u.Email == email);
        Assert.NotEqual(TestData.Password, stored.PasswordHash);
        Assert.False(string.IsNullOrWhiteSpace(stored.PasswordHash));
        Assert.DoesNotContain(TestData.Password, stored.PasswordHash);
    }

    [Fact]
    public async Task Register_participant_returns_201()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"part.{Guid.NewGuid():N}@raceday.co.za",
            password = TestData.Password,
            firstName = "Naledi",
            lastName = "Khumalo",
            role = "Participant"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_unknown_role_returns_400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"bad.{Guid.NewGuid():N}@raceday.co.za",
            password = TestData.Password,
            firstName = "X",
            lastName = "Y",
            role = "Admin"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_duplicate_email_returns_409()
    {
        var client = _factory.CreateClient();
        var payload = new
        {
            email = TestData.OrganiserEmail,
            password = TestData.Password,
            firstName = "Dup",
            lastName = "User",
            role = "Organiser"
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", payload);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_sets_session_with_user_id_and_role()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = TestData.OrganiserEmail,
            password = TestData.Password
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var session = await client.GetAsync("/api/auth/session");
        Assert.Equal(HttpStatusCode.OK, session.StatusCode);
        var body = await session.Content.ReadFromJsonAsync<SessionDto>();
        Assert.Equal("Organiser", body!.Role);
        Assert.True(body.UserId > 0);
        Assert.Equal(TestData.OrganiserEmail, body.Email);
    }

    [Fact]
    public async Task Login_wrong_password_returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = TestData.OrganiserEmail,
            password = "WrongPassword1!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Protected_route_without_session_returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_clears_session_so_next_call_is_401()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = TestData.ParticipantEmail,
            password = TestData.Password
        });

        var logout = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var me = await client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }

    [Fact]
    public async Task Logout_without_session_returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed class AuthUserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
    }

    private sealed class SessionDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
