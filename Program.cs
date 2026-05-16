using Microsoft.AspNetCore.Authentication.Cookies;
using NexFit.Backend.Data;
using NexFit.Backend.Data; // Agar aapka Data folder is namespace par hai toh isy use karein

var builder = WebApplication.CreateBuilder(args);

// Inject Single MongoDB Context instance
builder.Services.AddSingleton<MongoDbContext>();

// Inject MVC Controllers and Razor Views views support pipelines
builder.Services.AddControllersWithViews();

// Setup Secure Framework Managed App Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";        // Agar user authenticated nahi hai aur secure button par click karey ga toh yahan jayega
        options.AccessDeniedPath = "/Auth/Login"; // Agar user Admin nahi hai aur admin panel par janay ki koshish karey ga
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Track user authentication status before routing verification rules execute
app.UseAuthentication();
app.UseAuthorization();

// Default Route updated to Home/Index so the public dashboard opens first
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();