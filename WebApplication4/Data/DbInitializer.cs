using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Models;

namespace WebApplication4.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply migrations
        await context.Database.MigrateAsync();

        // Seed roles
        var roles = new[] { "Admin", "Client", "Agent" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed admin user
        var adminEmail = "admin@realestate.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator"
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed a sample client user
        var clientEmail = "client@realestate.com";
        var clientUser = await userManager.FindByEmailAsync(clientEmail);
        if (clientUser == null)
        {
            clientUser = new ApplicationUser
            {
                UserName = clientEmail,
                Email = clientEmail,
                EmailConfirmed = true,
                FirstName = "John",
                LastName = "Doe"
            };
            var result = await userManager.CreateAsync(clientUser, "Client@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(clientUser, "Client");
            }
        }

        // Seed a sample agent user
        var agentEmail = "agent@realestate.com";
        var agentUser = await userManager.FindByEmailAsync(agentEmail);
        if (agentUser == null)
        {
            agentUser = new ApplicationUser
            {
                UserName = agentEmail,
                Email = agentEmail,
                EmailConfirmed = true,
                FirstName = "Jane",
                LastName = "Smith"
            };
            var result = await userManager.CreateAsync(agentUser, "Agent@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(agentUser, "Agent");
            }
        }
    }
}
