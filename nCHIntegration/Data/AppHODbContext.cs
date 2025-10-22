using Microsoft.EntityFrameworkCore;
using nCHIntegration.Models;

namespace nCHIntegration.Data
{
    public class AppHODBContext : DbContext
    {
        public AppHODBContext(DbContextOptions<AppHODBContext> options) : base(options) { }
        public DbSet<PathologyResult> PathoResult { get; set; }
        public virtual DbSet<Patient> Patients { get; set; }



    }
}
