using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SUPFLY.Areas.Identity.Data; // Ensure this is the correct namespace for SUPFLYUser
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SUPFLY.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<SUPFLYUser>>();

            // 1. Define and Create Roles
            string[] roleNames = { "Admin", "Staff", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    // This creates the role in the AspNetRoles table
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Create the Default Admin User (Optional, but highly recommended)
            var adminEmail = "admin@supfly.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new SUPFLYUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Super",
                    LastName = "Admin",
                    EmailConfirmed = true
                };

                // Create the user with a strong, default password
                var result = await userManager.CreateAsync(newAdmin, "AdminP@ss123");

                if (result.Succeeded)
                {
                    // Assign the Admin role (now that it exists)
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}