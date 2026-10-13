
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrafficFineManagement.Models;

namespace TrafficFineManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; } = null!;

        public DbSet<TrafficFine> TrafficFines { get; set; } = null!;

        public DbSet<ApprovalHistory> ApprovalHistories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Araç plakası benzersiz olmalı.
            builder.Entity<Vehicle>()
                .HasIndex(v => v.PlateNumber)
                .IsUnique();

            // Bir araç birden fazla ceza alabilir.
            builder.Entity<TrafficFine>()
                .HasOne(f => f.Vehicle)
                .WithMany(v => v.TrafficFines)
                .HasForeignKey(f => f.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tutanak numarası benzersiz olmalı.
            builder.Entity<TrafficFine>()
                .HasIndex(f => f.FineNumber)
                .IsUnique();

            // Bir trafik cezasının birden fazla
            // onay geçmişi kaydı olabilir.
            builder.Entity<ApprovalHistory>()
                .HasOne(h => h.TrafficFine)
                .WithMany(f => f.ApprovalHistories)
                .HasForeignKey(h => h.TrafficFineId)
                .OnDelete(DeleteBehavior.Restrict);

            // Her onay işlemi bir Identity kullanıcısına bağlıdır.
            builder.Entity<ApprovalHistory>()
                .HasOne(h => h.User)
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
