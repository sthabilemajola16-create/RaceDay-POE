using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public static class SeedData
{
    public const string DemoPassword = "Password1!";

    /// <summary>
    /// If the Part 1 SQL script already ran, those rows used placeholder hashes.
    /// This re-hashes the four demo accounts so Swagger login works with Password1!.
    /// If the database is empty, it inserts the same South African sample as the SQL script.
    /// </summary>
    public static void EnsureDemoData(RaceDayContext db, IPasswordHasher<AppUser> hasher)
    {
        if (!db.Clubs.Any())
        {
            db.Clubs.AddRange(
                new Club { Name = "KwaZulu-Natal Athletics Club", Province = "KwaZulu-Natal", City = "Durban", FoundedYear = 1988 },
                new Club { Name = "Cape Town Road Cycling Club", Province = "Western Cape", City = "Cape Town", FoundedYear = 1995 });
            db.SaveChanges();
        }

        if (!db.AppUsers.Any())
        {
            var thabo = MakeUser(hasher, 1, "thabo.organiser@raceday.co.za", "Thabo", "Mokoena", Roles.Organiser, "+27821230001", new DateOnly(1984, 3, 12));
            var ayesha = MakeUser(hasher, 2, "ayesha.organiser@raceday.co.za", "Ayesha", "Davids", Roles.Organiser, "+27821230002", new DateOnly(1988, 11, 4));
            var lerato = MakeUser(hasher, 1, "lerato.runner@email.com", "Lerato", "Nkosi", Roles.Participant, "+27834560011", new DateOnly(1996, 7, 21));
            var pieter = MakeUser(hasher, 2, "pieter.cyclist@email.com", "Pieter", "Botha", Roles.Participant, "+27834560012", new DateOnly(1992, 1, 30));
            db.AppUsers.AddRange(thabo, ayesha, lerato, pieter);
            db.SaveChanges();
        }
        else
        {
            foreach (var email in new[]
                     {
                         "thabo.organiser@raceday.co.za",
                         "ayesha.organiser@raceday.co.za",
                         "lerato.runner@email.com",
                         "pieter.cyclist@email.com"
                     })
            {
                var user = db.AppUsers.FirstOrDefault(u => u.Email == email);
                if (user is not null)
                {
                    user.PasswordHash = hasher.HashPassword(user, DemoPassword);
                }
            }

            db.SaveChanges();
        }

        if (!db.RaceEvents.Any())
        {
            var organiser1 = db.AppUsers.First(u => u.Email == "thabo.organiser@raceday.co.za");
            var organiser2 = db.AppUsers.First(u => u.Email == "ayesha.organiser@raceday.co.za");

            db.RaceEvents.AddRange(
                new RaceEvent
                {
                    OrganiserId = organiser1.UserId,
                    Name = "Soweto Sunrise 10 km",
                    Description = "Community road run through Orlando and Vilakazi Street precinct.",
                    SportType = "Running",
                    Venue = "Orlando Stadium precinct",
                    City = "Soweto",
                    Province = "Gauteng",
                    StartDateTime = new DateTime(2026, 9, 20, 6, 30, 0),
                    EndDateTime = new DateTime(2026, 9, 20, 11, 0, 0),
                    Status = "Published",
                    RouteSummary = "Out-and-back on closed suburban roads. Water table at 5 km.",
                    CreatedAt = DateTime.UtcNow
                },
                new RaceEvent
                {
                    OrganiserId = organiser1.UserId,
                    Name = "Durban Beachfront Half Marathon",
                    Description = "Point to uShaka coastal 21.1 km.",
                    SportType = "Running",
                    Venue = "North Beach start, Durban",
                    City = "Durban",
                    Province = "KwaZulu-Natal",
                    StartDateTime = new DateTime(2026, 10, 11, 6, 0, 0),
                    EndDateTime = new DateTime(2026, 10, 11, 12, 30, 0),
                    Status = "Published",
                    RouteSummary = "Flat seafront route. Wind can be strong after 08:00.",
                    CreatedAt = DateTime.UtcNow
                },
                new RaceEvent
                {
                    OrganiserId = organiser2.UserId,
                    Name = "Stellenbosch Winelands Cycle Classic",
                    Description = "Sportive ride for the Western Cape cycling community.",
                    SportType = "Cycling",
                    Venue = "Markotter Stadium, Stellenbosch",
                    City = "Stellenbosch",
                    Province = "Western Cape",
                    StartDateTime = new DateTime(2026, 11, 8, 7, 0, 0),
                    EndDateTime = new DateTime(2026, 11, 8, 16, 0, 0),
                    Status = "Published",
                    RouteSummary = "Rolling farm roads. Feed zone at 42 km on the 80 km route.",
                    CreatedAt = DateTime.UtcNow
                });
            db.SaveChanges();
        }

        if (!db.Categories.Any())
        {
            var events = db.RaceEvents.OrderBy(e => e.EventId).ToList();
            db.Categories.AddRange(
                new Category { EventId = events[0].EventId, Name = "5 km Fun Run", DistanceKm = 5, EntryFeeZar = 80, MaxParticipants = 400, MinAge = 12 },
                new Category { EventId = events[0].EventId, Name = "10 km Open", DistanceKm = 10, EntryFeeZar = 150, MaxParticipants = 800, MinAge = 16 },
                new Category { EventId = events[1].EventId, Name = "10 km Companion Run", DistanceKm = 10, EntryFeeZar = 180, MaxParticipants = 500, MinAge = 16 },
                new Category { EventId = events[1].EventId, Name = "21.1 km Half Marathon", DistanceKm = 21.10m, EntryFeeZar = 280, MaxParticipants = 1500, MinAge = 18 },
                new Category { EventId = events[2].EventId, Name = "40 km Sportive", DistanceKm = 40, EntryFeeZar = 350, MaxParticipants = 600, MinAge = 16 },
                new Category { EventId = events[2].EventId, Name = "80 km Classic", DistanceKm = 80, EntryFeeZar = 520, MaxParticipants = 400, MinAge = 18 });
            db.SaveChanges();
        }
    }

    public static void EnsureCreatedAndSeed(RaceDayContext db, IPasswordHasher<AppUser> hasher)
    {
        db.Database.EnsureCreated();
        EnsureDemoData(db, hasher);
    }

    private static AppUser MakeUser(
        IPasswordHasher<AppUser> hasher,
        int clubId,
        string email,
        string first,
        string last,
        string role,
        string phone,
        DateOnly dob)
    {
        var user = new AppUser
        {
            ClubId = clubId,
            Email = email,
            FirstName = first,
            LastName = last,
            Role = role,
            Phone = phone,
            DateOfBirth = dob,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = "pending"
        };
        user.PasswordHash = hasher.HashPassword(user, DemoPassword);
        return user;
    }
}
