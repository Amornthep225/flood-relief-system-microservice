using FloodRelief.Models;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }

        public DbSet<Admin> Admins { get; set; }

        public DbSet<Center> Centers { get; set; }

        public DbSet<Staff> Staffs { get; set; }
    }
}
