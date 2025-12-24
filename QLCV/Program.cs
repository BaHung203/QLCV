using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Hubs;
using WebApp.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;
// ✅ 1️⃣ Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QLCONGVAN")));

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific", builder =>
    {
        builder.WithOrigins("https://localhost:7010", "https://docs.google.com") 
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ICongVanDenService, CongVanDenService>();
builder.Services.AddScoped<ICongVanDiService, CongVanDiService>();
builder.Services.AddScoped<INhanVienService, NhanVienService>();
builder.Services.AddScoped<ThongBaoService>();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowSpecific");

// ✅ 9️⃣ Bật Session
app.UseSession();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";

    // allowlist: login pages, register, static folders, signalr hub, health checks, swagger, api endpoints you want to exclude
    var allowPrefixes = new[]
    {
        "/login",      // allow /Login/Login, /Login/Register, /Login/Index
        "/css/",
        "/js/",
        "/lib/",
        "/assets/",
        "/favicon.ico",
        "/signalr",    // if you use SignalR path
        "/notificationhub",
        "/home/error"  // allow error page
    };

    bool isAllowed = allowPrefixes.Any(p => path.StartsWith(p));

    if (!isAllowed)
    {
        var role = context.Session.GetString("Role");
        if (string.IsNullOrEmpty(role))
        {
            // If AJAX request, you may want to return 401 JSON instead of redirect.
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" || context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            context.Response.Redirect("/Login/Login");
            return;
        }
    }

    await next();
});

app.UseAuthorization();

app.MapHub<NotificationHub>("/notificationHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
