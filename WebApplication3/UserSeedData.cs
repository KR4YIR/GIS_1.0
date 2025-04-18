using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using WebApplication3.Identities;

namespace WebApplication3
{
    public class UserSeedData
    {
        public static async Task Seed(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            var adminRole = await roleManager.FindByNameAsync("admin");
            if (adminRole is null)
            {
                await roleManager.CreateAsync(new AppRole { Name = "admin" });
            }

            var userRole = await roleManager.FindByNameAsync("user");
            if (userRole is null)
            {
                await roleManager.CreateAsync(new AppRole { Name = "user" });
            }

            var userRoleClaim = await roleManager.GetClaimsAsync(userRole!);

            if (!userRoleClaim.Any())
            {
                roleManager.AddClaimAsync(userRole, new Claim("update", "true"));
                roleManager.AddClaimAsync(userRole, new Claim("delete", "true"));
                roleManager.AddClaimAsync(userRole, new Claim("add", "true"));
            }

            

            var user = userManager.Users.FirstOrDefault();

            if (!await userManager.IsInRoleAsync(user, "user"))
            {
                userManager.AddToRoleAsync(user, "user");
            }

            
        }
    }
}
