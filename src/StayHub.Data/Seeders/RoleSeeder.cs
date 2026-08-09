using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StayHub.Core.Constants;
using StayHub.Core.Models;

namespace StayHub.Data.Seeders;

/// <summary>
/// Creates the four platform roles and the single SuperAdmin account on startup.
/// The SuperAdmin is the only account permitted to promote a Guest to Admin.
/// </summary>
public static class RoleSeeder
{
    /// <summary>
    /// Ensures every role in <see cref="Roles.All"/> exists and that the seeded
    /// SuperAdmin account is present. Safe to run on every startup.
    /// </summary>
    /// <param name="services">The application service provider.</param>
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var email = configuration["SeedUsers:SuperAdmin:Email"] ?? "superadmin@stayhub.co.za";
        var password = configuration["SeedUsers:SuperAdmin:Password"] ?? "SuperAdmin123!";

        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var superAdmin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "Super",
            LastName = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(superAdmin, password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed the SuperAdmin account: {errors}");
        }

        await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin);
    }
}
