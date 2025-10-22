using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace nCHIntegration.Models
{
    public class Patient
    {
        [Key]
        public Int32 PatientUID { get; set; }
        public string HN { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string MiddleName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime DOB { get; set; }
        public string Gender { get; set; } = null!;
        public string Insurance { get; set; } = null!;
        public string Payor { get; set; } = null!;
        public string Agreement { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string PACS { get; set; } = null!;
        public List<PathologyResult> PathologyResults { get; set; }
    }
    [PrimaryKey(nameof(PatientUID), nameof(HN), nameof(RequestNumber))]
    public class PathologyResult
    {
        public Int32 PatientUID { get; set; }
        public string HN { get; set; } = null!;
        public string RequestNumber { get; set; } = null!;
        public string RequestItemCode { get; set; } = null!;
        public string RequestItemName { get; set; } = null!;
        public DateTime RequestDate { get; set; }
        public DateTime ResultDate { get; set; }
        public string ResultValue { get; set; } = null!;
    }
}
