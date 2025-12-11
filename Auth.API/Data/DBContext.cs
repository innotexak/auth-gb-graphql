using Microsoft.EntityFrameworkCore;
using Auth.API.Entities;
namespace Auth.API.Data
{
    public class AuthDBContext(DbContextOptions<AuthDBContext> options) : DbContext(options)
    {
            public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasData(
                 new User
                 {
                     Id = Guid.NewGuid(),
                     Email = "user@example.com",
                     Password = "password123",
                     Username = "user123",
                     RefreshToken = "refreshToken",
                     RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7),
                     Role = "User"
                 }
                );
            
        }
    }
}
 