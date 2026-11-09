using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class DashboardController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index()
    {
        var role = HttpContext.Session.GetString("Role");
        if (string.IsNullOrWhiteSpace(role)) return RedirectToAction("Login", "Account");

        if (role == "Participant")
        {
            ViewBag.Enrolments = await api.GetAsync<List<Dictionary<string, object>>>("/api/enrolments/me") ?? new();
            ViewBag.Results = await api.GetAsync<List<Dictionary<string, object>>>("/api/results/me") ?? new();
        }
        else if (role == "Organiser")
        {
            ViewBag.Events = await api.GetAsync<List<Dictionary<string, object>>>("/api/events/mine") ?? new();
        }
        else return Forbid();

        return View();
    }
}
