using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities;
using Project.Domain.Entities.Event;
using Project.Domain.Entities.Product;
using Project.Domain.Entities.Users;
using Project.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor for dependency injection (recommended)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Parameterless constructor for EF Tools (migrations)
        public AppDbContext() : base()
        {
        }

        // Event-related entities
        public DbSet<Event> Events { get; set; }
        public DbSet<EventMetadata> EventMetadata { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }
        public DbSet<EventSchedule> EventSchedules { get; set; }

        // Product-related entities
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductPrice> ProductPrices { get; set; }
        public DbSet<ProductStock> ProductStocks { get; set; }

        // User-related entities
        public DbSet<User> Users { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }

        // Other domain entities
        public DbSet<Redemption> Redemptions { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Only configure if not already configured by dependency injection
            if (!optionsBuilder.IsConfigured)
            {
                // Try to get from environment variable first
                var connectionString = Environment.GetEnvironmentVariable("PROJECT_CONNECTION_STRING");

                if (string.IsNullOrEmpty(connectionString))
                {
                    // Fallback to default (for local development only)
                    connectionString = "Data Source=localhost\\SQLEXPRESS; Initial Catalog=Project_EfCore; Connect Timeout=60; Encrypt=True; Integrated Security=True;Persist Security Info=False;Pooling=False; TrustServerCertificate=True";
                }

                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<ProductPrice>()
                .HasKey(pp => pp.ProductId);

            modelBuilder.Entity<ProductStock>()
                .HasKey(ps => ps.ProductId);

            modelBuilder.Entity<UserAccount>()
                .HasKey(ua => ua.Id);

            modelBuilder.Entity<EventSchedule>()
                .HasKey(es => es.Id);

            modelBuilder.Entity<Event>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<EventParticipant>()
                .HasKey(ep => ep.Id);

            modelBuilder.Entity<EventMetadata>()
                .HasKey(em => em.Id);

            modelBuilder.Entity<Transaction>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Redemption>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserAccount)
                .WithOne(a => a.User)
                .HasForeignKey<UserAccount>(a => a.UserId);

            modelBuilder.Entity<Product>()
                .HasOne(u => u.ProductPrice)
                .WithOne(a => a.Product)
                .HasForeignKey<ProductPrice>(a => a.ProductId);

            modelBuilder.Entity<Product>()
                .HasOne(u => u.ProductStock)
                .WithOne(a => a.Product)
                .HasForeignKey<ProductStock>(a => a.ProductId);

            // One Event has many Participants
            modelBuilder.Entity<Event>()
                .HasMany(e => e.Participants)
                .WithOne(p => p.Event)
                .HasForeignKey(p => p.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Each participant belongs to one User or One user can participate in many events
            modelBuilder.Entity<EventParticipant>()
                .HasOne(ep => ep.User)
                .WithMany()
                .HasForeignKey(ep => ep.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // unique (EventId, UserId) pair
            modelBuilder.Entity<EventParticipant>()
                .HasIndex(ep => new { ep.EventId, ep.UserId })
                .IsUnique();

            // Event (1) <-> (1) EventSchedule
            modelBuilder.Entity<Event>()
                .HasOne(e => e.EventSchedule)
                .WithOne(s => s.Event)
                .HasForeignKey<EventSchedule>(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Event (1) <-> (1) EventMetadata
            modelBuilder.Entity<Event>()
                .HasOne(e => e.EventMetadata)
                .WithOne(m => m.Event)
                .HasForeignKey<EventMetadata>(m => m.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // EventMetadata (many) -> (1) User (Organizer)
            modelBuilder.Entity<EventMetadata>()
                .HasOne(m => m.Organizer)
                .WithMany()
                .HasForeignKey(m => m.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint on (EventId, OrganizerId)
            modelBuilder.Entity<EventMetadata>()
                .HasIndex(m => new { m.EventId, m.OrganizerId })
                .IsUnique();

            // Transaction relationships
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event relationship (optional - only for Event source)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Event)
                .WithMany()
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.NoAction);

            // EventParticipant relationship (optional - only for Event source)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.EventParticipant)
                .WithMany()
                .HasForeignKey(t => t.EventParticipantId)
                .OnDelete(DeleteBehavior.SetNull);

            // Admin approval relationship (optional - only for AdminAward source)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.AdminUser)
                .WithMany()
                .HasForeignKey(t => t.AdminApprovedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Transaction_AdminApproval");

            // Reversal admin relationship (optional)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ReversalAdmin)
                .WithMany()
                .HasForeignKey(t => t.ReversedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Transaction_ReversalAdmin");

            // Transaction indexes
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.UserId);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.EventId);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.TimeStamp);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => new { t.UserId, t.TimeStamp });

            // Redemption relationships - Approval admin
            modelBuilder.Entity<Redemption>()
                .HasOne(r => r.ApprovalAdmin)
                .WithMany()
                .HasForeignKey(r => r.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Redemption_ApprovalAdmin");

            // Redemption relationships - Rejection admin
            modelBuilder.Entity<Redemption>()
                .HasOne(r => r.RejectionAdmin)
                .WithMany()
                .HasForeignKey(r => r.RejectedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Redemption_RejectionAdmin");

            // Redemption relationships - User and Product
            modelBuilder.Entity<Redemption>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Redemption>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Redemption indexes
            modelBuilder.Entity<Redemption>()
                .HasIndex(r => r.UserId);

            modelBuilder.Entity<Redemption>()
                .HasIndex(r => r.Status);

            modelBuilder.Entity<Redemption>()
                .HasIndex(r => r.CreatedAt);

            // Redemption compound index (non-unique to allow multiple redemptions)
            modelBuilder.Entity<Redemption>()
                .HasIndex(r => new { r.UserId, r.ProductId })
                .IsUnique(false);
        }
    }
}
