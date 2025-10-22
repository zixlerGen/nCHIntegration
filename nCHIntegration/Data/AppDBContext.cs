using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using nCHIntegration.Models;

namespace nCHIntegration.Data
{
    public class AppDBContext: IdentityDbContext<Users>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }
        public DbSet<Users> Users { get; set; } = null!;
        public DbSet<CRA_Patient_Right> CRA_Patient_Right { get; set; } = null!;
        public DbSet<CRA_MDT_Appointment> CRA_MDT_Appointment { get; set; } = null!;
        public DbSet<CRA_MDT_Group> CRA_MDT_Group { get; set; } = null!;
        public DbSet<CRA_MDT_Doctor> CRA_MDT_Doctor { get; set; } = null!;
        public DbSet<CRA_MDT_Consult> CRA_MDT_Consult { get; set; } = null!;
        public DbSet<CRA_Temp_Patient_xRay> CRA_Temp_Patient_xRay { get; set; } = null!;
        public DbSet<CRA_Point_Of_Discussion> CRA_Point_Of_Discussion { get; set; } = null!;
        public DbSet<CRA_Temp_Patient_History> CRA_Temp_Patient_History { get; set; } = null!;
        public DbSet<CRA_Temp_Patient_Pathology> CRA_Temp_Patient_Pathology { get; set; } = null!;
        public DbSet<CRA_Temp_Patient_Lab> CRA_Temp_Patient_Lab { get; set; } = null!;

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    modelBuilder.Entity<CRA_MDT_Appointment>()
        //        .Property(e => e.MDT_Visit)
        //        .HasColumnName("mdt_visit"); // Verify this matches your DB column name
        //}
    }
}
