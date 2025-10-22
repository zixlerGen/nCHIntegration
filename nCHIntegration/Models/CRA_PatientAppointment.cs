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
        public Int32? Gender { get; set; }
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
        public string? appoint_by_doctor { get; set; }
        public string? appoint_by_doctor_name { get; set; }
        public DateTime Update_Date { get; set; }
        public string? Update_By { get; set; }
    }
    public class TbPatientLab
    {
        public Int32? ID { get; set; }
        public required string HN { get; set; }
        public string? Facility_RMS_No { get; set; }
        public required string Request_No { get; set; }
        public required string Lab_Code { get; set; }
        public required string Lab_Name_English { get; set; }
        public required string Result_Value { get; set; }
        public required string Lab_Unit { get; set; }
        public DateTime? Result_Datetime { get; set; }
        public string? Previous_Result_Value { get; set; }
        public DateTime? Previous_Result_Datetime { get; set; }
        public DateTime? Update_Date { get; set; }
        public string? Update_By { get; set; }
    }
    public class TbPatientXray
    {
        public Int32? ID { get; set; }
        public required string HN { get; set; }
        public string? Facility_RMS_No { get; set; }
        public required string Request_No { get; set; }
        public DateTime? Confirm_Result_Datetime { get; set; }
        public string? Rich_Text_Memo { get; set; }
        public DateTime? Update_Date { get; set; }
        public string? Update_By { get; set; }
        public DateTime? Date_Of_Xray { get; set; }
        public string? xray_code { get; set; }
        public string? xray_name_english { get; set; }
    }
    public class TbPatientPathologySurgical
    {
        public Int32? ID { get; set; }
        public required string HN { get; set; }
        public string? Facility_RMS_No { get; set; }
        public required string Request_No { get; set; }
        public DateTime? Specimen_Receive_Datetime { get; set; }
        public DateTime? Approve_Datetime { get; set; }
        public string? Rich_Text_Memo { get; set; }
        public DateTime? Update_Date { get; set; }
        public string? Update_By { get; set; }
        public string? Path_no { get; set; }
    }
    public class TbPatientPathologyCyto
    {
        public Int32? ID { get; set; }
        public required string HN { get; set; }
        public string? Facility_RMS_No { get; set; }
        public required string Request_No { get; set; }
        public DateTime? Specimen_Receive_Datetime { get; set; }
        public DateTime? Approve_Datetime { get; set; }
        public string? Rich_Text_Memo { get; set; }
        public DateTime? Update_Date { get; set; }
        public string? Update_By { get; set; }
        public string? Specimen_id { get; set; }
    }
    public class TbPatientPhysical
    {
        public Int32? ID { get; set; }
        public required string HN { get; set; }
        public string? Facility_RMS_No { get; set; }
        public required string Request_No { get; set; }
        public DateTime Update_Date { get; set; }
        public string? Update_By { get; set; }
    }
    public class TbPatientDiagnosis
    {
        public Int32? ID { get; set; }
        public string? HN { get; set; }
        public DateTime? Visit_Date { get; set; }
        public string? VN { get; set; }
        public string? Prescription_No { get; set; }
        public string? Diagnosis_Record_Type { get; set; }
        public string? ICD_Code { get; set; }
        public string? ICD_Text_EN { get; set; }
        public DateTime Update_Date { get; set; }
        public string? Update_By { get; set; }
    }
    public class TbPatientHistory
    {
        public Int32? ID { get; set; }
        public string? HN { get; set; }
        public DateTime? IN_Date { get; set; }
        public DateTime? Visit_date { get; set; }
        public string? VN { get; set; }
        public string? Subjective_Data { get; set; }
        public string? Plan_Of_Management { get; set; }
        public DateTime? Update_Date { get; set; }
        public string? Update_By { get; set; }
        public string? record_type { get; set; }
        public string? clinic_name { get; set; }
        public string? clinic { get; set; }
    }

}
