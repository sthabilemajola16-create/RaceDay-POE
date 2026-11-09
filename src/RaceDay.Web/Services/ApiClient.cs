using System.Net.Http.Headers;
namespace RaceDay.Web.Services;
public class ApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor) {
 public async Task<HttpResponseMessage> SendAsync(HttpMethod method,string path,HttpContent? content=null) { var c=factory.CreateClient("RaceDayApi"); var req=new HttpRequestMessage(method,path){Content=content}; var cookie=accessor.HttpContext?.Request.Headers.Cookie.ToString(); if(!string.IsNullOrWhiteSpace(cookie)) req.Headers.TryAddWithoutValidation("Cookie",cookie); return await c.SendAsync(req); }
 public async Task<T?> GetAsync<T>(string path) { using var r=await SendAsync(HttpMethod.Get,path); if(!r.IsSuccessStatusCode)return default; return await r.Content.ReadFromJsonAsync<T>(); }
}
