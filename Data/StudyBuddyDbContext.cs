using Microsoft.EntityFrameworkCore;
using StudyBuddyMobile.Models;

namespace StudyBuddyMobile.Data
{
    public class StudyBuddyDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<StudyResource> StudyResources { get; set; }
        public DbSet<StudyGroup> StudyGroups { get; set; }
        public DbSet<StudyGroupMember> StudyGroupMembers { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }

        public StudyBuddyDbContext()
        {
            // Ensure database is created
            Database.EnsureCreated();
        }

        // Helper method to reset database (for development)
        public static void ResetDatabase()
        {
            try
            {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "StudyBuddy.db");
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                }
            }
            catch { }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Get the path to store the database file
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "StudyBuddy.db");
            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.Name).IsRequired();
            });

            // Configure StudyResource entity
            modelBuilder.Entity<StudyResource>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Subject).IsRequired();
                entity.Property(e => e.ResourceType).IsRequired();

                // Configure relationship
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
            });

            // Configure StudyGroup entity
            modelBuilder.Entity<StudyGroup>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Subject).IsRequired();

                entity.HasOne(e => e.Creator)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);
            });

            // Configure StudyGroupMember entity
            modelBuilder.Entity<StudyGroupMember>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.StudyGroupId, e.UserId }).IsUnique();

                entity.HasOne(e => e.StudyGroup)
                      .WithMany(sg => sg.Members)
                      .HasForeignKey(e => e.StudyGroupId)
                      .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
            });

            // Configure GroupMessage entity
            modelBuilder.Entity<GroupMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired();

                entity.HasOne(e => e.StudyGroup)
                      .WithMany(sg => sg.Messages)
                      .HasForeignKey(e => e.StudyGroupId)
                      .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}