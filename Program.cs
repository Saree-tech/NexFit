using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using NexFit.Data;
using NexFit.Services;

var builder = WebApplication.CreateBuilder(args);

// =========================================
// SERVICES
// =========================================

builder.Services.AddControllersWithViews();

// =========================================
// FILE SIZE LIMIT
// =========================================

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit =
        50 * 1024 * 1024;
});

// =========================================
// HTTP CLIENTS
// =========================================

builder.Services.AddHttpClient<DietSnapService>(client =>
{
    client.Timeout =
        TimeSpan.FromMinutes(5);
});

builder.Services.AddHttpClient<PostureService>(client =>
{
    client.Timeout =
        TimeSpan.FromMinutes(5);
});

// =========================================
// MONGODB SERVICES
// =========================================

builder.Services.AddSingleton<MongoDbRepository>();

builder.Services.AddScoped<DashboardService>();

// =========================================
// AI MODULE SERVICES
// =========================================

builder.Services.AddScoped<DietSnapService>();

builder.Services.AddScoped<PostureService>();

builder.Services.AddScoped<WorkoutService>();

builder.Services.AddHttpClient();

// =========================================
// AUTHENTICATION
// =========================================

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)

    .AddCookie(options =>
    {
        options.Cookie.Name =
            "NexFit.Auth.Cookie";

        options.LoginPath =
            "/Auth/Login";

        options.AccessDeniedPath =
            "/Auth/Login";

        options.ExpireTimeSpan =
            TimeSpan.FromDays(7);

        options.SlidingExpiration =
            true;

        options.Cookie.HttpOnly =
            true;

        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;

        options.Cookie.SameSite =
            SameSiteMode.Lax;
    });

// =========================================
// AUTHORIZATION
// =========================================

builder.Services.AddAuthorization();

// =========================================
// BUILD APP
// =========================================

var app = builder.Build();

// =========================================
// SEED ADMIN USER
// =========================================

await AdminSeeder.SeedAdminAsync(
    app.Services);

// =========================================
// PIPELINE
// =========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// =========================================
// ROUTES
// =========================================

app.MapControllerRoute(
    name: "default",
    pattern:
    "{controller=Home}/{action=Index}/{id?}");

app.Run();