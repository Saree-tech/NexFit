using Microsoft.AspNetCore.Authentication.Cookies;
using NexFit.Services;

var builder = WebApplication.CreateBuilder(args);

// =========================
// SERVICES
// =========================

builder.Services.AddControllersWithViews();

// Mongo Services
builder.Services.AddSingleton<MongoDbRepository>();
builder.Services.AddScoped<DashboardService>();

// AI Module Services (Farkhanda - Module 1)
builder.Services.AddScoped<DietSnapService>();
builder.Services.AddScoped<PostureService>();
builder.Services.AddScoped<WorkoutService>();
builder.Services.AddHttpClient();

// =========================
// AUTHENTICATION (COOKIE)
// =========================

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "NexFit.Auth.Cookie";

        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";

        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// =========================
// AUTHORIZATION
// =========================

builder.Services.AddAuthorization();

var app = builder.Build();

// =========================
// PIPELINE
// =========================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// =========================
// ROUTES
// =========================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();