using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

/// <summary>
/// Code-first model that mirrors docs/RaceDay.sql (eight tables, same keys and checks).
/// </summary>
public class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options)
    {
    }

    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<RaceEvent> RaceEvents => Set<RaceEvent>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<RouteInfo> Routes => Set<RouteInfo>();
    public DbSet<WeatherSnapshot> WeatherSnapshots => Set<WeatherSnapshot>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Club>(entity =>
        {
            entity.ToTable("Club");
            entity.HasKey(e => e.ClubId);
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Province).HasMaxLength(50).IsRequired();
            entity.Property(e => e.City).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AppUser");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(80).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(e => e.Club)
                .WithMany(c => c.Members)
                .HasForeignKey(e => e.ClubId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RaceEvent>(entity =>
        {
            entity.ToTable("RaceEvent");
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SportType).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Venue).HasMaxLength(200).IsRequired();
            entity.Property(e => e.City).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Province).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.RouteSummary).HasMaxLength(500);
            entity.HasOne(e => e.Organiser)
                .WithMany(u => u.OrganisedEvents)
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.Name).HasMaxLength(80).IsRequired();
            entity.Property(e => e.DistanceKm).HasColumnType("decimal(6,2)");
            entity.Property(e => e.EntryFeeZar).HasColumnName("EntryFeeZAR").HasColumnType("decimal(10,2)");
            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.Categories)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RouteInfo>(entity =>
        {
            entity.ToTable("Route");
            entity.HasKey(e => e.RouteId);
            entity.Property(e => e.Name).HasMaxLength(120).IsRequired();
            entity.Property(e => e.DistanceKm).HasColumnType("decimal(6,2)");
            entity.Property(e => e.GpxUrl).HasMaxLength(500);
            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.Routes)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WeatherSnapshot>(entity =>
        {
            entity.ToTable("WeatherSnapshot");
            entity.HasKey(e => e.WeatherSnapshotId);
            entity.Property(e => e.TemperatureC).HasColumnType("decimal(4,1)");
            entity.Property(e => e.Condition).HasMaxLength(80).IsRequired();
            entity.Property(e => e.WindKph).HasColumnType("decimal(5,1)");
            entity.Property(e => e.Source).HasMaxLength(80).IsRequired();
            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.WeatherSnapshots)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.ToTable("Enrolment");
            entity.HasKey(e => e.EnrolmentId);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => new { e.EventId, e.ParticipantId }).IsUnique();
            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Participant)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.ToTable("Result");
            entity.HasKey(e => e.ResultId);
            entity.Property(e => e.ResultStatus).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.EnrolmentId).IsUnique();
            entity.HasOne(e => e.Enrolment)
                .WithOne(en => en.Result)
                .HasForeignKey<Result>(e => e.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.CapturedByOrganiser)
                .WithMany()
                .HasForeignKey(e => e.CapturedByOrganiserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
