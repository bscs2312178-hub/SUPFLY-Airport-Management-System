using Microsoft.AspNetCore.Identity;
using SUPFLY.Areas.Identity.Data; // Ensure this matches your project's Identity User class
using SUPFLY.Data; // Ensure this matches your DbContext namespace

namespace SUPFLY.Utilities
{
    public static class DbInitializer
    {
        // This is the main function we will call to set up the roles
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<SUPFLYUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Define Roles
            string[] roleNames = { "Admin", "Staff", "Passenger" };

            foreach (var roleName in roleNames)
            {
                // Create the role if it doesn't exist
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Create Default Admin User (If none exists)
            string adminEmail = "admin@supfly.com";
            string adminPassword = "Admin123!"; // CHANGE THIS LATER!

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new SUPFLYUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true // Confirm immediately for easy login
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    // 3. Assign Admin Role
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}