using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CorrectBonus.Data
{
    public class InstallerDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=.;Database=__TEMP__;Trusted_Connection=True;")
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}
