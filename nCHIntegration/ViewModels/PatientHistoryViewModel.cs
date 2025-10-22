using nCHIntegration.Models;

namespace nCHIntegration.ViewModels
{
    public class PatientHistoryViewModel
    {
        public List<TbPatientHistory> HistoryList { get; set; } = new List<TbPatientHistory>();
        public DateTime? AppointDatetime { get; set; }
        public DateTime? FilterDate { get; set; }
        public string? HN { get; set; }
    }
}

