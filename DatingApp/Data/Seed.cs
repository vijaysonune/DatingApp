using DatingApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DatingApp.Data
{
    public class Seed
    {
        public static async Task SeedData(UserManager<AppUser> userManager , RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;

            var userData = await System.IO.File.ReadAllTextAsync(@"DefaultData\UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            var roles = new List<AppRole>
            {
                new AppRole { Name="Member"},
                new AppRole { Name="Admin"},
                new AppRole { Name="Moderator"}
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }


            foreach (var user in users)
            {      
                
                user.UserName = user.UserName.ToLower();

                await userManager.CreateAsync(user, "Test@123");
                await userManager.AddToRoleAsync(user, "Member");
            }

            var admin = new AppUser
            {
                UserName = "admin"
            };

            await userManager.CreateAsync(admin, "Test@123");
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });

        }
    }
}
