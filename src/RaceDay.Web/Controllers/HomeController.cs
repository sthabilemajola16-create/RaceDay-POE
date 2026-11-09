using Microsoft.AspNetCore.Mvc; using RaceDay.Web.Services;
namespace RaceDay.Web.Controllers;
public class HomeController(ApiClient api):Controller { public async Task<IActionResult> Index(){ ViewBag.Events=await api.GetAsync<List<Dictionary<string,object>>>("/api/events"); return View(); } public IActionResult Error()=>View(); }
