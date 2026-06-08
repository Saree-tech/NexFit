using Microsoft.AspNetCore.Authentication.Cookies;
using NexFit.Data;
using NexFit.Hubs;
using NexFit.Services;

var builder = WebApplication.CreateBuilder(args);

// =========================================
// SERVICES
// =========================================

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// File size limit 50MB
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
});

// HttpClient with longer timeout for video upload
builder.Services.AddHttpClient<DietSnapService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddHttpClient<PostureService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
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
        options.Cookie.Name = "NexFit.Auth.Cookie";
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
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
    app.UseExceptionHandler("/Home/Error");
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

app.MapHub<ChatHub>("/chatHub");
app.MapHub<GymHub>("/gymHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();