using nCHIntegration.Models;

namespace nCHIntegration.ViewModels
{
    public class PatientImagingViewModel
    {
        public List<TbPatientXray> XrayList { get; set; } = new List<TbPatientXray>();
        //public DateTime? AppointDatetime { get; set; }
        public DateTime? FilterDate { get; set; }
        public string? HN { get; set; }
    }
}
