using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using nCHIntegration.Models;

namespace nCHIntegration.Data
{
    public class AppCRADbContext: DbContext
    {
        public AppCRADbContext(DbContextOptions<AppCRADbContext> options) : base(options) { }
        public DbSet<CRA_PatientAppointment> CRAPatientAppointment { get; set; }
    }
}
