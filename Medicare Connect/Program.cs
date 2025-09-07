using Medicare_Connect.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Medicare_Connect.Areas.Patients.Services;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddLocalization();
builder.Services.AddControllersWithViews();

// Email
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

// Timeslot and Appointment services
builder.Services.AddScoped<ITimeslotService, TimeslotService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// Stripe configuration
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

var app = builder.Build();

// Localization middleware
var supportedCultures = new[] { "en", "zu", "st", "ts" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed identity data (roles and users)
using (var scope = app.Services.CreateScope())
{
    try
    {
        await Medicare_Connect.Data.IdentityDataSeeder.SeedAsync(scope.ServiceProvider);
        await DemoDataSeeder.SeedAsync(scope.ServiceProvider);
        await TimeslotDataSeeder.SeedTimeslotsAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to seed data. The application will continue to run.");
    }
}

app.Run();
