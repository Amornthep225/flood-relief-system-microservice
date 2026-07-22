using FloodRelief.Models;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Center> Centers { get; set; }
        public DbSet<Staff> Staffs { get; set; }

        public DbSet<ReliefCategory> ReliefCategories { get; set; }
        public DbSet<ReliefItem> ReliefItems { get; set; }

        public DbSet<SosRequest> SosRequests { get; set; }
        public DbSet<SosRequestItem> SosRequestItems { get; set; }
        public DbSet<Donation> Donations { get; set; }

        public DbSet<DonationItem> DonationItems { get; set; }

        public DbSet<CenterInventory> CenterInventories { get; set; }

        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SosRequest>()
                .HasOne(x => x.User)
                .WithMany(x => x.SosRequests)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SosRequest>()
                .HasOne(x => x.Center)
                .WithMany(x => x.SosRequests)
                .HasForeignKey(x => x.CenterId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SosRequest>()
                .HasOne(x => x.AssignedStaff)
                .WithMany(x => x.AssignedSosRequests)
                .HasForeignKey(x => x.AssignedStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SosRequestItem>()
                .HasOne(x => x.SosRequest)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.SosRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SosRequestItem>()
                .HasOne(x => x.ReliefItem)
                .WithMany(x => x.SosRequestItems)
                .HasForeignKey(x => x.ReliefItemId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DonationItem>()
                .HasOne(x => x.Donation)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.DonationId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DonationItem>()
                .HasOne(x => x.ReliefItem)
                .WithMany()
                .HasForeignKey(x => x.ReliefItemId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Donation>()
                .HasOne(x => x.Center)
                .WithMany()
                .HasForeignKey(x => x.CenterId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Donation>()
                .HasOne(x => x.User)
                .WithMany(x => x.Donations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CenterInventory>()
                .HasIndex(x => new
                {
                    x.CenterId,
                    x.ReliefItemId
                })
                .IsUnique();
            modelBuilder.Entity<CenterInventory>()
                .HasIndex(x => new
                {
                    x.CenterId,
                    x.ReliefItemId
                })
                .IsUnique();

            modelBuilder.Entity<CenterInventory>()
                .HasOne(x => x.Center)
                .WithMany(x => x.Inventories)
                .HasForeignKey(x => x.CenterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CenterInventory>()
                .HasOne(x => x.ReliefItem)
                .WithMany()
                .HasForeignKey(x => x.ReliefItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(x => x.CenterInventory)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.CenterInventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(x => x.Staff)
                .WithMany(x => x.InventoryTransactions)
                .HasForeignKey(x => x.StaffId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}