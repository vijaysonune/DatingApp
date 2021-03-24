using DatingApp.Entities;
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
        public static async Task SeedData(DataContext dataContext)
        {
            if (await dataContext.Users.AnyAsync()) return;

            var userData = await System.IO.File.ReadAllTextAsync(@"Migrations\UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            foreach(var user in users)
            {
                using var hmac = new HMACSHA512();

                user.UserName = user.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Test@123"));
                user.PasswordSalt = hmac.Key;

                dataContext.Users.Add(user);
            }
           await dataContext.SaveChangesAsync();
        }
    }
}
