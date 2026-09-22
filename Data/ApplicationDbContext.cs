using HeThongHienMauTichHopAI.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongHienMauTichHopAI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Campaign> Campaigns { get; set; }

        public DbSet<DonationRegistration>
            DonationRegistrations { get; set; }
    }
}