using Microsoft.AspNetCore.Authentication.Cookies;
using NexFit.Data;
using NexFit.Services;

var builder = WebApplication.CreateBuilder(args);

// =========================================
// SERVICES
// =========================================

builder.Services.AddControllersWithViews();

<<<<<<< HEAD
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

// Mongo Services
=======
// =========================================
// MONGODB SERVICES
// =========================================

>>>>>>> 9a6cabef447f949fae3b4b426f59a93e1f76bf1f
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
<<<<<<< HEAD
        options.Cookie.Name = "NexFit.Auth.Cookie";
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
=======
        options.Cookie.Name =
            "NexFit.Auth.Cookie";

        options.LoginPath =
            "/Auth/Login";

        options.AccessDeniedPath =
            "/Auth/Login";

        options.ExpireTimeSpan =
            TimeSpan.FromDays(7);

>>>>>>> 9a6cabef447f949fae3b4b426f59a93e1f76bf1f
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;

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

app.MapControllerRoute(
    name: "default",
    pattern:
    "{controller=Home}/{action=Index}/{id?}");

app.Run();
