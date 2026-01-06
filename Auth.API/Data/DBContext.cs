using Microsoft.EntityFrameworkCore;
using Auth.API.Entities;
using System;

namespace Auth.API.Data
{
    public class AuthDBContext : DbContext
    {
        public AuthDBContext(DbContextOptions<AuthDBContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Authentication> Authentication => Set<Authentication>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
        public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();

        public DbSet<Group> Groups => Set<Group>();



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------------
            // Configure Relationships
            // -------------------------------


            modelBuilder.Entity<User>()
            .OwnsOne(u => u.Preferences, pref =>
            {
                pref.Property(p => p.EmailNotification).HasMaxLength(10);
                pref.Property(p => p.ProfileVisibility).HasMaxLength(10);
                // Configure other properties as needed
            });
            // Authentication 1:1 User
            modelBuilder.Entity<Authentication>()
                .HasOne(a => a.User)
                .WithOne(u => u.Authentication)
                .HasForeignKey<Authentication>(a => a.UserId);

            // Post N:1 User
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------------
            // Seed Data
            // -------------------------------

            var userId = Guid.NewGuid();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            // User seed
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = userId,
                    Email = "user@gmail.com",
                    Password = "password123",
                    Username = "user123",
                    Avatar = "default-avatar.png",
                    Bio = "Software Developer"
                }
            );

    
            // Authentication seed — only set FK, navigation property remains null
            modelBuilder.Entity<Authentication>().HasData(
                new Authentication
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Role = "User",
                    RefreshToken = "refreshToken",
                    RefreshTokenExpiryTime = refreshTokenExpiry
                }
            );

            // Post seed — only set FK, navigation property remains null
            modelBuilder.Entity<Post>().HasData(
                new Post
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = "Hello World",
                    Description = "First Post",
                    Content = "This is the content"
                }
            );


            // -------------------------------
            // Chat / Direct Message Models
            // -------------------------------

            // Conversation
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Type)
                      .IsRequired();

                entity.HasIndex(c => c.Type);
                     
            });

            // ConversationParticipant
            modelBuilder.Entity<ConversationParticipant>(entity =>
            {
                entity.HasKey(cp => cp.Id);

                entity.HasOne(cp => cp.Conversation)
                      .WithMany(c => c.Participants)
                      .HasForeignKey(cp => cp.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cp => cp.User)
                      .WithMany()
                      .HasForeignKey(cp => cp.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(cp => new { cp.ConversationId, cp.UserId })
                      .IsUnique();
            });

            // DirectMessage
            modelBuilder.Entity<DirectMessage>(entity =>
            {
                entity.HasKey(dm => dm.Id);

                entity.HasOne(dm => dm.Conversation)
                      .WithMany(c => c.Messages)
                      .HasForeignKey(dm => dm.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(dm => dm.Sender)
                      .WithMany()
                      .HasForeignKey(dm => dm.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(dm => dm.Content)
                      .IsRequired();

                entity.HasIndex(dm => dm.ConversationId);
            });

            // Enforce uniqueness of title in group table for a particular user 
            modelBuilder.Entity<Group>()
              .HasIndex(g => new { g.UserId, g.Title })
              .IsUnique();

        }
    }
}
