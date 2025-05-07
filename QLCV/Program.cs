using Microsoft.EntityFrameworkCore;
using WebApp.Data; // Đảm bảo đúng namespace chứa AppDbContext

var builder = WebApplication.CreateBuilder(args);

// Đăng ký AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QLCONGVAN")));

builder.Services.AddControllersWithViews();

// Thêm cấu hình Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Thời gian timeout của session (ví dụ: 20 phút)
    options.Cookie.HttpOnly = true; // Ngăn JavaScript phía client truy cập cookie session
    options.Cookie.IsEssential = true; // Cho biết cookie này là cần thiết để session hoạt động
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

// Thêm Session middleware vào pipeline
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();