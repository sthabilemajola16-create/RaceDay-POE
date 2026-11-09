using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using System.Net.Http.Json;

namespace RaceDay.Web.Controllers;
public class ParticipantController(ApiClient api) : Controller
{
    [HttpPost] [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        if (HttpContext.Session.GetString("Role") != "Participant") return Forbid();
        using var r = await api.SendAsync(HttpMethod.Post, "/api/enrolments", JsonContent.Create(new { eventId, categoryId }));
        TempData["Notice"] = r.IsSuccessStatusCode ? "You are entered! Check My dashboard for your bib number." : "Enrolment could not be completed. You may already be entered or the category may be full.";
        return RedirectToAction("Details", "Events", new { id = eventId });
    }
}
