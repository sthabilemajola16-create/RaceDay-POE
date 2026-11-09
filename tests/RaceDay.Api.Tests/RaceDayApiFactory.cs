using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests;

public class RaceDayApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            var leftover = services
                .Where(d => d.ServiceType == typeof(RaceDayContext)
                            || d.ServiceType == typeof(DbContextOptions<RaceDayContext>)
                            || (d.ServiceType.IsGenericType
                                && d.ServiceType.GetGenericArguments().Any(t => t == typeof(RaceDayContext))))
                .ToList();
            foreach (var descriptor in leftover)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<RaceDayContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
        db.Database.EnsureCreated();
        TestData.Seed(db, hasher);
        return host;
    }
}

public static class TestData
{
    public const string Password = "Password1!";
    public const string OrganiserEmail = "organiser@test.co.za";
    public const string OtherOrganiserEmail = "other.organiser@test.co.za";
    public const string ParticipantEmail = "participant@test.co.za";

    public static void Seed(RaceDayContext db, IPasswordHasher<AppUser> hasher)
    {
        if (db.AppUsers.Any())
        {
            return;
        }

        var club = new Club { Name = "Test Club", Province = "Gauteng", City = "Johannesburg", FoundedYear = 2000 };
        db.Clubs.Add(club);
        db.SaveChanges();

        var organiser = User(hasher, club.ClubId, OrganiserEmail, "Thabo", "Test", Roles.Organiser);
        var other = User(hasher, club.ClubId, OtherOrganiserEmail, "Ayesha", "Test", Roles.Organiser);
        var participant = User(hasher, club.ClubId, ParticipantEmail, "Lerato", "Test", Roles.Participant);
        db.AppUsers.AddRange(organiser, other, participant);
        db.SaveChanges();

        var ev = new RaceEvent
        {
            OrganiserId = organiser.UserId,
            Name = "Test 10 km",
            Description = "Fixture event for enrolment tests.",
            SportType = "Running",
            Venue = "Test Park",
            City = "Soweto",
            Province = "Gauteng",
            StartDateTime = new DateTime(2026, 9, 20, 6, 30, 0),
            EndDateTime = new DateTime(2026, 9, 20, 11, 0, 0),
            Status = "Published",
            RouteSummary = "Out and back.",
            CreatedAt = DateTime.UtcNow
        };
        db.RaceEvents.Add(ev);
        db.SaveChanges();

        db.Categories.Add(new Category
        {
            EventId = ev.EventId,
            Name = "10 km Open",
            DistanceKm = 10,
            EntryFeeZar = 150,
            MaxParticipants = 800,
            MinAge = 16
        });
        db.SaveChanges();
    }

    private static AppUser User(
        IPasswordHasher<AppUser> hasher,
        int clubId,
        string email,
        string first,
        string last,
        string role)
    {
        var user = new AppUser
        {
            ClubId = clubId,
            Email = email,
            FirstName = first,
            LastName = last,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = "pending"
        };
        user.PasswordHash = hasher.HashPassword(user, Password);
        return user;
    }
}
