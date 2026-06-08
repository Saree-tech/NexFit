using Microsoft.AspNetCore.Authentication.Cookies;
using NexFit.Services;
using NexFit.MLModel;

var builder = WebApplication.CreateBuilder(args);

// =========================
// SERVICES
// =========================

builder.Services.AddControllersWithViews();

// File size limit 50MB
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
});

// HttpClient with longer timeout
builder.Services.AddHttpClient<DietSnapService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddHttpClient<PostureService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});

// Mongo Services
builder.Services.AddSingleton<MongoDbRepository>();
builder.Services.AddScoped<DashboardService>();

// AI Module Services (Farkhanda - Module 1)
builder.Services.AddScoped<FoodClassifier>();   // <-- NEW
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
// ML MODEL TRAINING
// Terminal mein: dotnet run --train
// Training ke baad ye block apne aap skip hoga
// =========================

if (args.Contains("--train"))
{
    var datasetPath = Path.Combine(Directory.GetCurrentDirectory(), "FoodDataset");
    var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModel", "food_model.zip");
    NexFit.MLModel.TrainModel.Train(datasetPath, modelPath);
    Console.WriteLine("\nTraining complete! Ab normal run karo: dotnet run");
    return;
}

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