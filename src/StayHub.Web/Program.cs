using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;
using StayHub.Data;
using StayHub.Data.Repositories;
using StayHub.Data.Seeders;
using StayHub.Web.Hubs;
using StayHub.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Data access
// ---------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("StayHubDB")
    ?? throw new InvalidOperationException("Connection string 'StayHubDB' not found in appsettings.json.");

builder.Services.AddDbContext<StayHubDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ---------------------------------------------------------------------------
// Identity with role support
// ---------------------------------------------------------------------------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<StayHubDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// ---------------------------------------------------------------------------
// Repositories and application services
// ---------------------------------------------------------------------------
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IHostApplicationRepository, HostApplicationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ICheckInRepository, CheckInRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAmenityRepository, AmenityRepository>();
builder.Services.AddScoped<INewsletterRepository, NewsletterRepository>();

builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IRoleTransitionService, RoleTransitionService>();
builder.Services.AddScoped<IMessagingPolicyService, MessagingPolicyService>();
builder.Services.AddScoped<IRealtimeDispatchService, RealtimeDispatchService>();

builder.Services.AddSingleton<IUserIdProvider, IdentityUserIdProvider>();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ---------------------------------------------------------------------------
// Apply migrations and seed roles, the SuperAdmin, and demo data
// ---------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<StayHubDbContext>();
    await context.Database.MigrateAsync();

    await RoleSeeder.SeedAsync(scope.ServiceProvider);
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

// ---------------------------------------------------------------------------
// HTTP pipeline
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<StayHubHub>("/hubs/stayhub");

app.Run();
