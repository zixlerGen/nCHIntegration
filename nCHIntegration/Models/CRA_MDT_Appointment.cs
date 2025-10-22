using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace nCHIntegration.Models
{
    [PrimaryKey(nameof(HN))] // Move the PrimaryKey attribute to the class level
    public class CRA_MDT_Appointment
    {
        public string? HN { get; set; }
        public DateTime Appoint_Datetime { get; set; }
        public string? Initial_Name { get; set; }
        public string? First_Name { get; set; }
        public string? Last_Name { get; set; }
        public string? Gender_Name { get; set; }
        public DateTime Birth_Datetime { get; set; }
        public string? Right_Code { get; set; }
        public string? Right_Name { get; set; }
        public string? Province_Name { get; set; }
        public string? Doctor_Name { get; set; }
        public string? CTB_No { get; set; }
        public string? Remarks_Memo { get; set; }
        public DateTime? Confirm_Date { get; set; }
        public string? Confirm_By { get; set; }
        public bool Generate_File { get; set; }
        public string? MDT_Group { get; set; }
        public int? MDT_Visit { get; set; }
        public string? MDT_Consult { get; set; }
        public string? MDT_Review_Image { get; set; }
        public string? MDT_Appointment { get; set; }
        public DateTime? Update_Date { get; set; }
        public string? Update_By { get; set; }
        public bool Confirm_Status { get; set; }
        public string? Diagnostic_Name { get; set; }
        public bool Imaging_Status { get; set; }
        public bool Patho_Status { get; set; }
        public bool History_Status { get; set; }
        public string? Patient_Right_Group { get; set; }
        public string? appoint_by_doctor { get; set; }
        public string? appoint_by_doctor_name { get; set; }
        public string? Point_Of_Discussion { get; set; }
        public string? MDT_Meeting_Name { get; set; }
    }
    public class CRA_Patient_Right
    {
        [Key]
        public string? Right_Code { get; set; }
        public string? Right_Name { get; set; }
    }
    public class CRA_MDT_Group
    {
        [Key]
        public string? MDTGroup_Code { get; set; }
        public string? MDTGroup_Name { get; set; }
    }
    public class CRA_MDT_Consult
    {
        [Key]
        public DateTime Appoint_Datetime { get; set; }
        public string? Doctor_Consult { get; set; }
    }
    public class CRA_MDT_Doctor
    {
        [Key]
        public string? Doctor_Code { get; set; }
        public string? Doctor_Name { get; set; }
        public string? Doctor_Prefix { get; set; }
        public string? Doctor_Firstname { get; set; }
        public string? Doctor_Lastname { get; set; }
        public string? Doctor_Specialty { get; set; }
        public string? Doctor_Email { get; set; }
    }
    public class GenerateNavifyRequest
    {
        // บอก Deserializer ว่าให้ map JSON key 'selectedPatients' มาที่ property นี้
        [JsonPropertyName("selectedPatients")]
        public List<SelectedPatient> SelectedPatients { get; set; }
    }

    public class SelectedPatient
    {
        [JsonPropertyName("hn")]
        public string Hn { get; set; }

        [JsonPropertyName("mdtDate")]
        public string MdtDate { get; set; }
    }
    public class CRA_Temp_Patient_xRay
    {
        public DateTime? Xray_Date { get; set; }
        [Key]
        public string? Request_No { get; set; }
        public string? HN { get; set; }
        public DateTime? Confirm_Result_Datetime { get; set; }
        public string? Rich_Text_Memo { get; set; }
        public DateTime? Confirm_Date { get; set; }
        public string? Confirm_by { get; set; }
        public string? xray_code { get; set; }
        public string? xray_name_english { get; set; }
    }
    public class CRA_Point_Of_Discussion
    {
        [Key]
        public string? ID { get; set; }
        public string? Point_Of_Discussion { get; set; }

    }
    [PrimaryKey(nameof(MDT_Date), nameof(HN), nameof(MDT_Name))]
    public class CRA_Temp_Patient_History
    {
        public DateTime? MDT_Date { get; set; }
        public string? HN { get; set; }
        public string? patient_history { get; set; }
        public DateTime? Update_Date { get; set; }
        public string? Update_by { get; set; }
        public string? MDT_Name { get; set; }
    }
    [PrimaryKey(nameof(specimen_receive_datetime), nameof(HN), nameof(path_no))]
    public class CRA_Temp_Patient_Pathology
    {
        public string? HN { get; set; }
        public string? facility_rms_no { get; set; }
        public string? request_no { get; set; }
        public DateTime? specimen_receive_datetime { get; set; }
        public DateTime? approve_datetime { get; set; }
        public string? rich_text_memo { get; set; }
        public DateTime? update_date { get; set; }
        public string? update_by { get; set; }
        public string? path_no { get; set; }
        public string? lab_code { get; set; }
        public string? lab_name_english { get; set; }
        public string? histo_type { get; set; }

    }
    [PrimaryKey(nameof(HN), nameof(request_no), nameof(lab_code))]
    public class CRA_Temp_Patient_Lab
    {
        public string? HN { get; set; }
        public string? request_no { get; set; }
        public string? lab_code { get; set; }
        public string? lab_name_english { get; set; }
        public string? result_value { get; set; }
        public DateTime? result_datetime { get; set; }
        public string? previous_result_value { get; set; }
        public DateTime? previous_result_datetime { get; set; }
        public string? lab_unit { get; set; }
        public DateTime? update_date { get; set; }
        public string? update_by { get; set; }
        public string? request_lab_code { get; set; }
        public string? request_lab_name_english { get; set; }

    }

    public class CRA_Data_Sendout
    {
        public DateTime? Created_DateTime { get; set; }
        public string? Data_Domain { get; set; }
        public string? HN { get; set; }
        public string? Filename { get; set; }
        public string? File_Content { get; set; }
        public DateTime? Send_Datetime { get; set; }
        public string? Send_Status { get; set; } //Success, Failed
        public string? Send_Response { get; set; }
        public DateTime? MDT_Date { get; set; }
    }
}