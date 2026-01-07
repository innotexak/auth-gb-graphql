using Auth.API.Data;
using Microsoft.AspNetCore.Identity;
using Auth.API.Entities;

namespace Auth.API.Seed
{
    public static class AdminSeeder
    {
            public static async Task SeedSuperAdminAsync(
                AuthDBContext db,
                IPasswordHasher<User> passwordHasher)
            {
                // Check if any user exists
                if (!db.Users.Any())
                {
                    var superAdmin = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = "superadmin",
                        Email = "innotexdev@gmail.com",
                        Avatar = "default-avatar.png",
                        Bio = "Super Administrator",
           
                    };

                    superAdmin.Password = passwordHasher.HashPassword(superAdmin, "123456");

                    db.Users.Add(superAdmin);

                    // Optional: create Authentication entry
                    db.Authentication.Add(new Authentication
                    {
                        Id = Guid.NewGuid(),
                        UserId = superAdmin.Id,
                        Role = "SuperAdmin",
                    });

                    await db.SaveChangesAsync();
                }
            }

    }
}
