using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using System.Net.Http.Json;

namespace RaceDay.Web.Controllers;

public class EventsController(ApiClient api) : Controller
{
    private bool IsOrganiser => HttpContext.Session.GetString("Role") == "Organiser";
    public async Task<IActionResult> Index()
    {
        var data = await api.GetAsync<List<Dictionary<string, object>>>("/api/events");
        return View(data ?? new());
    }
    public async Task<IActionResult> Details(int id)
    {
        var data = await api.GetAsync<Dictionary<string, object>>($"/api/events/{id}");
        if (data == null) return NotFound();
        ViewBag.EventId = id;
        return View(data);
    }
    [HttpGet] public IActionResult Create()
    {
        if (!IsOrganiser) return Forbid();
        return View(new EventVm());
    }
    [HttpPost] [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventVm vm)
    {
        if (!IsOrganiser) return Forbid();
        if (!ModelState.IsValid) return View(vm);
        using var r = await api.SendAsync(HttpMethod.Post, "/api/events", JsonContent.Create(vm));
        if (!r.IsSuccessStatusCode) { ModelState.AddModelError("", "The API rejected the event. Check the date, required fields and organiser session."); return View(vm); }
        TempData["Notice"] = "Your event was created.";
        return RedirectToAction("Index");
    }
    [HttpGet] public async Task<IActionResult> Edit(int id)
    {
        if (!IsOrganiser) return Forbid();
        var e = await api.GetAsync<EventVm>($"/api/events/{id}");
        if (e == null) return NotFound();
        return View(e);
    }
    [HttpPost] [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventVm vm)
    {
        if (!IsOrganiser) return Forbid();
        if (!ModelState.IsValid) return View(vm);
        using var r = await api.SendAsync(HttpMethod.Put, $"/api/events/{id}", JsonContent.Create(vm));
        if (!r.IsSuccessStatusCode) { ModelState.AddModelError("", "Event update failed. Check the event values."); return View(vm); }
        TempData["Notice"] = "Event updated.";
        return RedirectToAction("Details", new { id });
    }
    [HttpPost] [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsOrganiser) return Forbid();
        using var r = await api.SendAsync(HttpMethod.Delete, $"/api/events/{id}");
        TempData["Notice"] = r.IsSuccessStatusCode ? "Event deleted." : "The event could not be deleted. It may already have enrolments.";
        return RedirectToAction("Index");
    }
    public async Task<IActionResult> Enrolments(int id)
    {
        if (!IsOrganiser) return Forbid();
        ViewBag.EventId = id;
        ViewBag.Event = await api.GetAsync<Dictionary<string, object>>($"/api/events/{id}");
        var data = await api.GetAsync<List<Dictionary<string, object>>>($"/api/events/{id}/enrolments");
        return View(data ?? new());
    }
    [HttpGet] public IActionResult Categories(int id)
    {
        if (!IsOrganiser) return Forbid();
        ViewBag.EventId = id;
        return View();
    }
    [HttpPost] [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(int id, string name, decimal distanceKm, decimal entryFeeZar, int? minAge, int? maxAge, int? maxParticipants)
    {
        if (!IsOrganiser) return Forbid();
        var body = new { name, distanceKm, entryFeeZar, minAge, maxAge, maxParticipants };
        using var r = await api.SendAsync(HttpMethod.Post, $"/api/events/{id}/categories", JsonContent.Create(body));
        TempData["Notice"] = r.IsSuccessStatusCode ? "Category added." : "Category could not be added. Check the values.";
        return RedirectToAction("Categories", new { id });
    }
    [HttpPost] [ValidateAntiForgeryToken]
    public async Task<IActionResult> CaptureResult(int id, int enrolmentId, string? finishTime, int? positionOverall, string resultStatus = "Finished")
    {
        if (!IsOrganiser) return Forbid();
        var body = new { enrolmentId, finishTime, positionOverall, resultStatus };
        using var r = await api.SendAsync(HttpMethod.Post, "/api/results", JsonContent.Create(body));
        TempData["Notice"] = r.IsSuccessStatusCode ? "Result captured." : "Result was not saved. Check that the enrolment has no existing result.";
        return RedirectToAction("Enrolments", new { id });
    }
}
