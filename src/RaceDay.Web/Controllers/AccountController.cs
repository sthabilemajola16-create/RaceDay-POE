using Microsoft.AspNetCore.Mvc; using RaceDay.Web.Models; using RaceDay.Web.Services; using System.Net.Http.Json;
namespace RaceDay.Web.Controllers;
public class AccountController(ApiClient api):Controller {
 [HttpGet] public IActionResult Login()=>View(new LoginVm());
 [HttpPost] public async Task<IActionResult> Login(LoginVm vm){ if(!ModelState.IsValid)return View(vm); using var r=await api.SendAsync(HttpMethod.Post,"/api/auth/login",JsonContent.Create(vm)); if(!r.IsSuccessStatusCode){ModelState.AddModelError("","Login failed. Check your email and password.");return View(vm);} if(r.Headers.TryGetValues("Set-Cookie",out var cookies)) foreach(var c in cookies) Response.Headers.Append("Set-Cookie",c); var data=await r.Content.ReadFromJsonAsync<Dictionary<string,object>>()??new(); HttpContext.Session.SetString("Email",vm.Email); HttpContext.Session.SetString("Role",data.GetValueOrDefault("role")?.ToString()??""); return RedirectToAction("Index","Home"); }
 [HttpGet] public IActionResult Register()=>View(new RegisterVm());
 [HttpPost] public async Task<IActionResult> Register(RegisterVm vm){if(!ModelState.IsValid)return View(vm); using var r=await api.SendAsync(HttpMethod.Post,"/api/auth/register",JsonContent.Create(vm)); if(!r.IsSuccessStatusCode){ModelState.AddModelError("","Registration could not be completed. Check your details or use another email.");return View(vm);} TempData["Notice"]="Account created. Please sign in.";return RedirectToAction("Login");}
 [HttpPost] public async Task<IActionResult> Logout(){using var r=await api.SendAsync(HttpMethod.Post,"/api/auth/logout");HttpContext.Session.Clear();Response.Headers.Append("Set-Cookie",".RaceDay.Session=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; httponly");return RedirectToAction("Index","Home");}
}
