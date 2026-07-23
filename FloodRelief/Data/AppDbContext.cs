using FloodRelief.Models;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;
        public DbSet<Center> Centers { get; set; } = null!;
        public DbSet<Staff> Staffs { get; set; } = null!;

        public DbSet<ReliefCategory> ReliefCategories { get; set; } = null!;
        public DbSet<ReliefItem> ReliefItems { get; set; } = null!;

        public DbSet<SosRequest> SosRequests { get; set; } = null!;
        public DbSet<SosRequestItem> SosRequestItems { get; set; } = null!;

        public DbSet<Donation> Donations { get; set; } = null!;
        public DbSet<DonationItem> DonationItems { get; set; } = null!;

        public DbSet<CenterInventory> CenterInventories { get; set; } = null!;
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;

        public DbSet<ThaiGeography> ThaiGeographies { get; set; } = null!;
        public DbSet<ThaiProvince> ThaiProvinces { get; set; } = null!;
        public DbSet<ThaiDistrict> ThaiDistricts { get; set; } = null!;
        public DbSet<ThaiSubDistrict> ThaiSubDistricts { get; set; } = null!;

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureSos(modelBuilder);
            ConfigureDonations(modelBuilder);
            ConfigureInventory(modelBuilder);
            ConfigureThaiAddresses(modelBuilder);
        }

        private static void ConfigureSos(
            ModelBuilder modelBuilder)
        {
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
        }

        private static void ConfigureDonations(
            ModelBuilder modelBuilder)
        {
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
        }

        private static void ConfigureInventory(
            ModelBuilder modelBuilder)
        {
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

        private static void ConfigureThaiAddresses(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ThaiProvince>()
                .HasOne(x => x.Geography)
                .WithMany(x => x.Provinces)
                .HasForeignKey(x => x.GeographyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ThaiProvince>()
                .HasIndex(x => x.NameTh);

            modelBuilder.Entity<ThaiDistrict>()
                .HasIndex(x => x.ProvinceId);

            modelBuilder.Entity<ThaiSubDistrict>()
                .HasIndex(x => x.DistrictId);

            modelBuilder.Entity<ThaiDistrict>()
                .HasOne(x => x.Province)
                .WithMany(x => x.Districts)
                .HasForeignKey(x => x.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ThaiSubDistrict>()
                .HasOne(x => x.District)
                .WithMany(x => x.SubDistricts)
                .HasForeignKey(x => x.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Center>()
                .HasOne(x => x.ProvinceData)
                .WithMany(x => x.Centers)
                .HasForeignKey(x => x.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Center>()
                .HasOne(x => x.DistrictData)
                .WithMany(x => x.Centers)
                .HasForeignKey(x => x.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Center>()
                .HasOne(x => x.SubDistrictData)
                .WithMany(x => x.Centers)
                .HasForeignKey(x => x.SubDistrictId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
