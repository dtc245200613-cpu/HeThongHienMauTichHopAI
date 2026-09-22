using HeThongHienMauTichHopAI.Data;
using HeThongHienMauTichHopAI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// MVC
// ========================================
builder.Services.AddControllersWithViews();

// ========================================
// DATABASE SQLITE
// ========================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=hienmau.db"));

// ========================================
// AI SERVICE
// ========================================
builder.Services.AddHttpClient<AIService>();

builder.Services.AddScoped<AIService>();

// ========================================
// SESSION
// ========================================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ========================================
// AUTHENTICATION
// ========================================
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ========================================
// TẠO TÀI KHOẢN MẪU
// ========================================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    // ------------------------------------
    // TÀI KHOẢN COORDINATOR
    // ------------------------------------
    if (!context.Users.Any(
        x => x.Email == "dpv@gmail.com"))
    {
        context.Users.Add(
            new HeThongHienMauTichHopAI.Models.User
            {
                FullName = "Điều phối viên",
                Email = "dpv@gmail.com",
                Password = "123456",
                Role = "Coordinator"
            });
    }

    // ------------------------------------
    // TÀI KHOẢN ADMIN
    // ------------------------------------
    if (!context.Users.Any(
        x => x.Email == "admin@gmail.com"))
    {
        context.Users.Add(
            new HeThongHienMauTichHopAI.Models.User
            {
                FullName = "Quản trị viên",
                Email = "admin@gmail.com",
                Password = "123456",
                Role = "Admin"
            });
    }

    context.SaveChanges();
}

// ========================================
// CONFIGURATION
// ========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ========================================
// HTTPS
// ========================================
app.UseHttpsRedirection();

// ========================================
// STATIC FILES
// ========================================
app.UseStaticFiles();

// ========================================
// ROUTING
// ========================================
app.UseRouting();

// ========================================
// SESSION
// ========================================
app.UseSession();

// ========================================
// AUTHENTICATION
// ========================================
app.UseAuthentication();

// ========================================
// AUTHORIZATION
// ========================================
app.UseAuthorization();

// ========================================
// DEFAULT ROUTE
// ========================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();