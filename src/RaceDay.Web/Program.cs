var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<RaceDay.Web.Services.ApiClient>();
builder.Services.AddHttpClient("RaceDayApi", client => client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5188"));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => { o.Cookie.Name = ".RaceDay.Web"; o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true; o.IdleTimeout = TimeSpan.FromHours(8); });
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseSession();
app.MapControllerRoute(name:"default", pattern:"{controller=Home}/{action=Index}/{id?}");
app.Run();