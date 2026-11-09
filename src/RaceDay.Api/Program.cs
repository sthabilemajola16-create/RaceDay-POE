using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromHours(8);
});
builder.Services.AddSingleton<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

builder.Services.AddDbContext<RaceDayContext>(options =>
{
    if (!builder.Environment.IsEnvironment("Testing"))
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDay"));
    }
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description =
            "ASP.NET Core Web API for the RaceDay system. Log in first (POST /api/auth/login) — a server-side session cookie then carries your user ID and role. Organiser and Participant routes reject the other role with 403. Passwords are hashed."
    });

    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml))
    {
        options.IncludeXmlComments(xml, includeControllerXmlComments: true);
    }
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
    options.DocumentTitle = "RaceDay API";
});

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseSession();
app.MapControllers();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
    try
    {
        SeedData.EnsureCreatedAndSeed(db, hasher);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex,
            "Could not create or seed SQL Server. Run docs/RaceDay.sql in SSMS, then restart. Demo login is Password1!.");
    }
}

app.Run();

public partial class Program;
