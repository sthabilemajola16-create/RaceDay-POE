using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Controllers;

/// <summary>Club list for the registration drop-down.</summary>
[Route("api/clubs")]
public class ClubsController : RaceDayControllerBase
{
    public ClubsController(RaceDayContext db) : base(db)
    {
    }

    /// <summary>Lists clubs (KZN Athletics, Cape Town Road Cycling, and any others in the database).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClubResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List()
    {
        var items = await Db.Clubs
            .OrderBy(c => c.Name)
            .Select(c => new ClubResponse
            {
                ClubId = c.ClubId,
                Name = c.Name,
                Province = c.Province,
                City = c.City,
                FoundedYear = c.FoundedYear
            })
            .ToListAsync();
        return Ok(items);
    }
}
