using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HeThongHienMauTichHopAI.Data
{
    public class DesignTimeDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder =
                new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseSqlite(
                "Data Source=hienmau.db");

            return new ApplicationDbContext(
                optionsBuilder.Options);
        }
    }
}