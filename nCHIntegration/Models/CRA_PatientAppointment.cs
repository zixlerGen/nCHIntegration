using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nCHIntegration.Models
{
    public class CRA_PatientAppointment
    {
        public required string HN { get; set; }
        public DateTime Appoint_Datetime { get; set; }
        public required string Appoint_No { get; set; }
        public required string Clinic_Code { get; set; }
        public required string Clinic_Name { get; set; }
        public string? Initial_Name_Code { get; set; }
        public string? Initial_Name { get; set; }
        public string? First_Name { get; set; }
        public string? Last_Name { get; set; }
        public string? Gender { get; set; }
        public string? Gender_Name { get; set; }
        public DateTime Birth_Datetime { get; set; }
        public string? Right_Code { get; set; }
        public string? Right_Name { get; set; }
        public string? Address1 { get; set; }
        public string? Province { get; set; }
        public string? Province_Name { get; set; }
        public string? Amphoe { get; set; }
        public string? Amphoe_Name { get; set; }
        public string? Tambon { get; set; }
        public string? Tambon_Name { get; set; }
        public string? Postal_Code { get; set; }
        public string? Doctor { get; set; }
        public string? Doctor_Name { get; set; }
        public string? CTB_No { get; set; }
        public string? Remarks_Memo { get; set; }
        public DateTime Update_Date { get; set; }
        public string? Update_By { get; set; }
    }
}
