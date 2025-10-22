using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using nCHIntegration.Models;
using Org.BouncyCastle.Utilities.Collections;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using RtfPipe.Model;
using RtfPipe.Tokens;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Text;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nCHIntegration.DIP
{
    public class CRA_DIP_FFMessage
    {
        private readonly CRA_MDT_Appointment _craMDTAppointment;
        private string _gender = string.Empty;
        private string _messageid = string.Empty;
        private string _messageDateTimeNow = string.Empty;
        private string _messageDateTime = string.Empty;

        public CRA_DIP_FFMessage(CRA_MDT_Appointment craMDTAppointment)
        {
            _craMDTAppointment = craMDTAppointment;
        }
        public CRA_DIP_FFMessage() { }
        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public string ConvertPdfToBase64(string pdfFilePath)
        {
            try
            {
                // Read the PDF file as a byte array
                byte[] pdfBytes = File.ReadAllBytes(pdfFilePath);

                // Convert the byte array to a Base64 string
                string base64String = Convert.ToBase64String(pdfBytes);

                return base64String;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting PDF to Base64: {ex.Message}");
                return string.Empty;
            }
        }
        public async Task<string> ConvertRtfToPlainTextAndPdfAsync(string rtfContent, string outputPdfPath)
        {
            // Step 1: Extract plain text from RTF
            //string plainText = ExtractPlainTextFromRtf(rtfContent);
            string plainText = RtfPipe.Rtf.ToHtml(rtfContent);

            //await ConvertHtmlToPdfAsync(plainText, outputPdfPath);
            // Step 2: Create a PDF with the plain text

            await GeneratePdfFromHtml(plainText, outputPdfPath);

            //await ConvertHtmlToPdfAsync(plainText, outputPdfPath);
            // Step 3: Return the plain text
            string pdfPlainText = ConvertPdfToBase64(outputPdfPath);

            return pdfPlainText;
        }
        public static async Task GeneratePdfFromHtml(string htmlContent, string outputPath)
        {
            // 1. Download Chromium browser
            // With the following corrected code:
            await new BrowserFetcher().DownloadAsync();
            // 2. Launch a new browser instance
            using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }))
            {
                // 3. Create a new page
                using (var page = await browser.NewPageAsync())
                {
                    // 4. Set the HTML content of the page
                    await page.SetContentAsync(htmlContent);

                    // 5. Generate a PDF from the page
                    await page.PdfAsync(outputPath, new PdfOptions { Format = PaperFormat.A4 });
                }
            }
        }
        public string CRA_FF_PatientDemographic()
        {
            string gender = _craMDTAppointment.Gender_Name ?? string.Empty; // Ensure null safety
            if (gender == "ชาย") _gender = "male";
            else if (gender == "หญิง") _gender = "female";

            string FFMessage = @"SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PrimaryIdentifier,PrimaryIdentifierSystem,LastName,FirstName,BirthDate,GenderCode" + "\r";
                        FFMessage += @"CLB,CLB_FLATFILE,nCH," + _craMDTAppointment.HN + ",CLB," +
                         (_craMDTAppointment.Last_Name ?? string.Empty) + "," +
                         (_craMDTAppointment.First_Name ?? string.Empty) + "," +
                         _craMDTAppointment.Birth_Datetime.ToString("yyyy-MM-dd", new CultureInfo("en-US")) + "," + _gender;
            return FFMessage;
        }
        public string CRA_FF_Appointment(string mdtType, string pointOfDiscussion, string discussionNote, string doctorFirstname, string doctorLastname, string SpecialityCode, string specialityCodeDisplay)
        {
            string FFMessage = @"SourceOrgIdentifier,SourceSystemIdentifier,PatientPrimaryIdentifier,PatientPrimaryIdentifierSystem,AppointmentIdentifier,AppointmentStatus,AppointmentType,AppointmentReason,AppointmentDescription,AppointmentRequestedby,AppointmentRequestedbyPrefix,AppointmentRequestedbySuffix,AppointmentRequestedbyFamilyName,AppointmentRequestedbyGivenName,AppointmentRequestedbyid,AppointmentSpecialityCode,AppointmentSpecialityCodeSystem,AppointmentSpecialityCodeDisplay" + "\r";
            FFMessage += @"CLB" + "," + //SourceOrgIdentifier
                "CLB_FLATFILE" + "," +//SourceSystemIdentifier
                _craMDTAppointment.HN + "," + //PatientPrimaryIdentifier
                "CLB" + "," + //PatientPrimaryIdentifierSystem
                "APPT-" + mdtType + "-" + _craMDTAppointment.HN + "-" + _craMDTAppointment.Appoint_Datetime.ToString("yyyy-MM-dd", new CultureInfo("en-US")) + "," +// AppointmentIdentifier
                "proposed" + "," +//AppointmentStatus
                mdtType + "," +// AppointmentType
                pointOfDiscussion + "," + // AppointmentReason
                discussionNote + "," +//  AppointmentDescription
                "yes" + "," + //AppointmentRequestedby
                "Dr." + "," +// AppointmentRequestedbyPrefix
                "MD" + "," +// AppointmentRequestedbySuffix
                doctorFirstname + "," +// AppointmentRequestedbyFamilyName
                doctorLastname + "," +// AppointmentRequestedbyGivenName
                doctorFirstname + "-" + doctorLastname + "," +// AppointmentRequestedbyid
                SpecialityCode + "," +// AppointmentSpecialityCode
                "http://snomed.info/sct" + "," +// AppointmentSpecialityCodeSystem
                specialityCodeDisplay; // AppointmentSpecialityCodeDisplay

            return FFMessage;
        }
        public string CRA_FF_AppointmentComments(string mdtType, string pointOfDiscussion, string discussionNote, string doctorFirstname, string doctorLastname, 
            string SpecialityCode, string specialityCodeDisplay, string AppointmentComments, string AppointmentPatientNote)
        {
            string FFMessage = @"SourceOrgIdentifier,SourceSystemIdentifier,PatientPrimaryIdentifier,PatientPrimaryIdentifierSystem,AppointmentIdentifier,AppointmentStatus,AppointmentType,AppointmentReason,AppointmentDescription,AppointmentRequestedby,AppointmentRequestedbyPrefix,AppointmentRequestedbySuffix,AppointmentRequestedbyFamilyName,AppointmentRequestedbyGivenName,AppointmentRequestedbyid,AppointmentSpecialityCode,AppointmentSpecialityCodeSystem,AppointmentSpecialityCodeDisplay,AppointmentComments,AppointmentPatientNote" + "\r";
            FFMessage += @"CLB" + "," + //SourceOrgIdentifier
                "CLB_FLATFILE" + "," +//SourceSystemIdentifier
                _craMDTAppointment.HN + "," + //PatientPrimaryIdentifier
                "CLB" + "," + //PatientPrimaryIdentifierSystem
                "APPT-" + mdtType + "-" + _craMDTAppointment.HN + "-" + DateTime.Now.ToString("yyyyMMddHHmmss", new CultureInfo("en-US")) + "," +// AppointmentIdentifier
                "proposed" + "," +//AppointmentStatus
                mdtType + "," +// AppointmentType
                pointOfDiscussion + "," + // AppointmentReason
                discussionNote + "," +//  AppointmentDescription
                "yes" + "," + //AppointmentRequestedby
                "" + "," +// AppointmentRequestedbyPrefix
                "" + "," +// AppointmentRequestedbySuffix
                doctorFirstname + "," +// AppointmentRequestedbyFamilyName
                doctorLastname + "," +// AppointmentRequestedbyGivenName
                doctorFirstname + "-" + doctorLastname + "," +// AppointmentRequestedbyid
                SpecialityCode + "," +// AppointmentSpecialityCode
                "http://snomed.info/sct" + "," +// AppointmentSpecialityCodeSystem
                specialityCodeDisplay + "," + // AppointmentSpecialityCodeDisplay
                AppointmentComments + "," + // AppointmentComments
                AppointmentPatientNote; // AppointmentPatientNote

            return FFMessage;
        }

        public string CRA_FF_Condition(string diagnostics, string diagnosticsDate, string updateDate)
        {
            string FFMessage = @"ResourceType,LastUpdatedTime,SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PatientPrimaryIdentifier,PatientPrimaryIdentifierSystem,ConditionIdentifier,ConditionIdentifierSystem,VerficationStatus,StatusCode,ConditionText,OnsetDateStart,Comment,PrimaryConditionCode,PrimaryConditionCodeSystem,PrimaryConditionCodeDisplay" + "\r";
            FFMessage += @"Condition" + "," + //ResourceType
                updateDate + "," +//LastUpdatedTime YYYY-MM-dd
                "CLB" + "," + //SourceOrgIdentifier
                "CLB_FLATFILE" + "," + //SourceSystemIdentifier
                "nCH" + "," + //DestinationSystemIdentifier
                _craMDTAppointment.HN + "," + //PatientPrimaryIdentifier
                "CLB" + "," + //PatientPrimaryIdentifierSystem
                "Cond" + _craMDTAppointment.HN + "_" + diagnosticsDate + "," + //ConditionIdentifier
                "" + "," + //ConditionIdentifierSystem
                "" + "," + //VerficationStatus
                "" + "," + //StatusCode
                "\""+diagnostics + "\"," + //ConditionText
                diagnosticsDate + "," + //OnsetDateStart"  YYYY-MM-DD+
                "" + "," + //Comment
                "3" + "," + //PrimaryConditionCode
                "urn:oid:2.16.840.1.113883.3.520.3.14" + "," + //PrimaryConditionCodeSystem
                "\"Malignant, primary site\""; //PrimaryConditionCodeDisplay

            return FFMessage;
        }
        public string CRA_FF_DocumentReference()
        {
            string insurance = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("สิทธการรักษา: " + _craMDTAppointment.Patient_Right_Group));
            string FFMessage = @"ResourceType,SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PatientPrimaryIdentifier,PatientPrimaryIdentifierSystem,DocumentReferenceMasterIdentifier,DocumentReferenceMasterIdentifierSystem,DocumentReferenceIdentifier,DocumentReferenceIdentifierSystem,DocumentReferenceStatus,InlineDocStatus,TypeCode,TypeCodeSystem,TypeDisplay,CategoryCode,CategoryCodeSystem,CategoryCodeDisplay,Date,Description,AttachmentContent,AttachmentContentMimeType" + "\r";
            FFMessage += @"DocumentReference" + "," + //ResourceType
                "CLB" + "," + //SourceOrgIdentifier
                "CLB_FLATFILE" + "," + //SourceSystemIdentifier
                "nCH" + "," + //DestinationSystemIdentifier
                _craMDTAppointment.HN + "," + //PatientPrimaryIdentifier
                "CLB" + "," + //PatientPrimaryIdentifierSystem
                _craMDTAppointment.HN + "Summ" + "," + //DocumentReferenceMasterIdentifier
                "" + "," + //DocumentReferenceMasterIdentifierSystem
                _craMDTAppointment.HN + "Summ" + "," + //DocumentReferenceIdentifier
                "" + "," + //DocumentReferenceIdentifierSystem
                "current" + "," + //DocumentReferenceStatus
                "final" + "," + //InlineDocStatus
                "34109-9" + "," + //TypeCode
                "LOINC" + "," + //TypeCodeSystem
                "Patient summary" + "," + //TypeDisplay
                "60591-5" + "," + //CategoryCode
                "LOINC" + "," + //CategoryCodeSystem
                "Patient summary" + "," + //CategoryCodeDisplay
                "2024-07-15" + "," + //Date
                "Patient Rights" + "," + //Description
                insurance + "," + //AttachmentContent
                "text/plain"; //AttachmentContentMimeType

            return FFMessage;
        }
        public string CRA_FF_PATDiagnosticReport(string pathoReport, string specimenReceiveDate, string requestno, string pathno, string lab_name)
        {
            string pathoReportText = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(pathoReport));
            string FFMessage = @"ResourceType,SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PatientPrimaryIdentifier,PatientPrimaryIdentifierSystem,DiagnosticPrimaryIdentifier,DiagnosticPrimaryIdentifierSystem,PrimaryReportStatus,DiagnosticReportCodeText,EffectiveDateTime,DiagnosisCategory,DiagnosisCategorySystem,DiagnosisCategoryDisplay,AttachmentName,AttachmentContent,AttachmentContentMimeType,Issued" + "\r";
            FFMessage += @"DiagnosticReport" + "," + //ResourceType
                "CLB" + "," + //SourceOrgIdentifier
                "CLB_FLATFILE" + "," + //SourceSystemIdentifier
                "nCH" + "," + //DestinationSystemIdentifier
                _craMDTAppointment.HN + "," + //PatientPrimaryIdentifier
                "CLB" + "," + //PatientPrimaryIdentifierSystem
                requestno + "_" + pathno + "," + //DiagnosticPrimaryIdentifier
                "" + "," + //DiagnosticPrimaryIdentifierSystem
                "final" + "," + //PrimaryReportStatus
                requestno + "_" + pathno + "," + //DiagnosticReportCodeText
                specimenReceiveDate + "," + //EffectiveDateTime
                "PAT" + "," + //DiagnosisCategory
                "http://terminology.hl7.org/CodeSystem/v2-0074" + "," + //DiagnosisCategorySystem
                "Pathology Report" + "," + //DiagnosisCategoryDisplay
                requestno + "_" + pathno +".txt" + "," +//AttachmentName
                pathoReportText + "," + //AttachmentContent
                "text/plain" + "," + //AttachmentContentMimeType
                specimenReceiveDate; //Issued

            return FFMessage;
        }
        public string CRA_FF_RADDiagnosticReport(string reportContent, string reportDate, string requestno, string xray_name)
        {
            string FFMessage = @"ResourceType,SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PatientPrimaryIdentifier,PatientPrimaryIdentifierSystem,DiagnosticPrimaryIdentifier,DiagnosticPrimaryIdentifierSystem,PrimaryReportStatus,DiagnosticReportCodeText,EffectiveDateTime,DiagnosisCategory,DiagnosisCategorySystem,DiagnosisCategoryDisplay,AttachmentName,AttachmentContent,AttachmentContentMimeType,Issued" + "\r";
            FFMessage += @"DiagnosticReport" + "," + //ResourceType
                "CLB" + "," + //SourceOrgIdentifier
                "CLB_FLATFILE" + "," + //SourceSystemIdentifier
                "nCH" + "," + //DestinationSystemIdentifier
                _craMDTAppointment.HN + "," + //PatientPrimaryIdentifier
                "CLB" + "," + //PatientPrimaryIdentifierSystem
                requestno + "_" + xray_name + "," + //DocumentReferenceMasterIdentifier
                "" + "," + //DocumentReferenceIdentifierSystem
                "final" + "," + //DocumentReferenceStatus
                requestno + "_" + xray_name + "," + //DiagnosticReportCodeText
                reportDate + "," + //EffectiveDateTime
                "RAD" + "," + //DiagnosisCategory
                "http://terminology.hl7.org/CodeSystem/v2-0074" + "," + //DiagnosisCategorySystem
                "Radiology Report" + "," + //DiagnosisCategoryDisplay
                requestno + "_" + xray_name + ".txt" + "," + //AttachmentName
                Base64Encode(reportContent) + "," + //AttachmentContent
                "text/plain" + "," + //AttachmentContentMimeType
                reportDate; //Issued

            return FFMessage;
        }

        public string CRA_FF_Observation_PhysicalExam(string effectiveDate, string obsevationCodeText, string observationValue)
        {
            string FFMessage = @"SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PatientPrimaryIdentifier,PatientPrimaryIdentifierSystem,PrimaryObservationIdentifier,PrimaryObservationIdentifierSystem,ObservationTypeCategoryCode,ObservationTypeCategoryCodeSystem,ObservationTypeCategoryCodeDisplay,ObservationCode,ObservationCodeSystem,ObservationCodeDisplay,ObservationCodeText,ObservationStatusCode,EffectiveDateTime,ValueString" + "\r";
            FFMessage += @"CLB" + "," + //SourceOrgIdentifier
                "CLB" + "," + //SourceSystemIdentifier
                "nCH" + "," + //DestinationSystemIdentifier
                _craMDTAppointment.HN + "," + //PatientPrimaryIdentifier
                "CLB" + "," + //PatientPrimaryIdentifierSystem
                "PE-" + _craMDTAppointment.HN + "-" + effectiveDate + "-" + obsevationCodeText + "," + //PrimaryObservationIdentifier
                "" + "," + //PrimaryObservationIdentifierSystem
                "vital-signs" + "," + //ObservationTypeCategoryCode
                "http://terminology.hl7.org/CodeSystem/observation-category" + "," + //ObservationTypeCategoryCodeSystem
                "vital-signs" + "," + //ObservationTypeCategoryCodeDisplay
                "" + "," + //ObservationCode
                "" + "," + //ObservationCodeSystem
                "" + "," + //ObservationCodeDisplay
                obsevationCodeText + "," + //ObservationCodeText
                "final" + "," + //ObservationStatusCode
                effectiveDate + "," + //EffectiveDateTime
                observationValue; //ValueString

            return FFMessage;
        }
        public string CRA_FF_LabBioObservation(string requestno, string testName, string testResult, string testInterpretation, string testNote, string methodCode, string methodDisplay, string resultDate)
        {
            string FFMessage = @"SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PatientPrimaryIdentifier,PatientPrimaryIdentifierSystem,PrimaryObservationIdentifier,PrimaryObservationIdentifierSystem,ObservationTypeCategoryCode,ObservationTypeCategoryCodeSystem,ObservationTypeCategoryCodeDisplay,ObservationCode,ObservationCodeSystem,ObservationCodeDisplay,ObservationCodeText,ObservationStatusCode,EffectiveDateTime,Issued,ValueString,InterpretationCodeSystem,InterpretationCode,InterpretationCodeDisplay,InterpretationText,Note,MethodCodeSystem,MethodCode,MethodCodeDisplay,MethodText" + "\r";
            FFMessage += @"CLB" + "," + //SourceOrgIdentifier
                "CLB" + "," + //SourceSystemIdentifier
                "nCH" + "," + //DestinationSystemIdentifier
                _craMDTAppointment.HN + "," + //PatientPrimaryIdentifier
                "CLB" + "," + //PatientPrimaryIdentifierSystem
                "LB-" + _craMDTAppointment.HN + "-" + requestno + "-" + testName + "," + //PrimaryObservationIdentifier
                "" + "," + //PrimaryObservationIdentifierSystem
                "cco-lab-biomarkers" + "," + //ObservationTypeCategoryCode
                "http://navify.com/fhir/CodeSystem/ObservationCategory" + "," + //ObservationTypeCategoryCodeSystem
                "Lab Biomarkers" + "," + //ObservationTypeCategoryCodeDisplay
                "10011-5" + "," + //ObservationCode
                "http://loinc.org" + "," + //ObservationCodeSystem
                testName + "," + //ObservationCodeDisplay
                testName + "," + //ObservationCodeText
                "final" + "," + //ObservationStatusCode
                resultDate + "," + //EffectiveDateTime
                resultDate + "," + //Issued
                testResult + "," + //ValueString
                "" + "," + //InterpretationCodeSystem
                "" + "," + //InterpretationCode
                "" + "," + //InterpretationCodeDisplay
                testInterpretation + "," + //InterpretationText
                testNote + "," + //Note
                methodCode + "," + //MethodCodeSystem
                methodCode + "," + //MethodCode
                methodDisplay + "," + //MethodCodeDisplay
                methodDisplay; //MethodText

            return FFMessage;
        }
        public string CRA_FF_HistoryOfIllness(string historyDate, string historyText, string historyAuthor)
        {
            string FFMessage = @"ResourceType,SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PatientPrimaryIdentifier,PatientPrimaryIdentifierSystem,DocumentReferenceMasterIdentifier,DocumentReferenceMasterIdentifierSystem,DocumentReferenceIdentifier,DocumentReferenceIdentifierSystem,DocumentReferenceStatus,InlineDocStatus,TypeCode,TypeCodeSystem,TypeDisplay,CategoryCode,CategoryCodeSystem,CategoryCodeDisplay,Date,Description,AttachmentContent,AttachmentContentMimeType,AuthorDisplay" + "\r";
            FFMessage += @"DocumentReference" + "," + //ResourceType
                "CLB" + "," + //SourceOrgIdentifier
                "CLB_FLATFILE" + "," + //SourceSystemIdentifier
                "nCH" + "," + //DestinationSystemIdentifier
                _craMDTAppointment.HN + "," + //PatientPrimaryIdentifier
                "CLB" + "," + //PatientPrimaryIdentifierSystem
                "hpi-" + _craMDTAppointment.HN + "-" + historyDate + "," + //DocumentReferenceMasterIdentifier
                "" + "," + //DocumentReferenceMasterIdentifierSystem
                "hpi-" + _craMDTAppointment.HN + "-" + historyDate + "," + //DocumentReferenceIdentifier
                "" + "," + //DocumentReferenceIdentifierSystem
                "current" + "," + //DocumentReferenceStatus
                "final" + "," + //InlineDocStatus
                "34109-09" + "," + //TypeCode
                "LOINC" + "," + //TypeCodeSystem
                "Note" + "," + //TypeDisplay
                "10164-2" + "," + //CategoryCode
                "LOINC" + "," + //CategoryCodeSystem
                "History of Present illness Narrative" + "," + //CategoryCodeDisplay
                historyDate + "," + //Date
                "History Of present illness description" + "," + //Description
                Base64Encode(historyText) + "," + //AttachmentContent
                "text/plain" + "," + //AttachmentContentMimeType
                historyAuthor; //AuthorDisplay

            return FFMessage;
        }
        public string CRA_FF_PatientAddIdentified(string ctbno)
        {
            string FFMessage = @"SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PrimaryIdentifier,PrimaryIdentifierSystem,SecondaryIdentifier,SecondaryIdentifierSystem,LastName,FirstName,BirthDate,GenderCode" + "\r";
            FFMessage += @"CLB" + "," + //SourceOrgIdentifier
                "CLB_FLATFILE" + "," + //SourceSystemIdentifier
                "nCH" + "," + //DestinationSystemIdentifier
                _craMDTAppointment.HN + "," + //PatientPrimaryIdentifier
                "CLB" + "," + //PatientPrimaryIdentifierSystem
                ctbno + "," + //SecondaryIdentifier
                "CTB" + "," + //SecondaryIdentifierSystem
                (_craMDTAppointment.Last_Name ?? string.Empty) + "," + //LastName
                (_craMDTAppointment.First_Name ?? string.Empty) + "," + //FirstName
                _craMDTAppointment.Birth_Datetime.ToString("yyyy-MM-dd", new CultureInfo("en-US")) + "," + //BirthDate
                _gender + "\r"; //GenderCode

            return FFMessage;
        }
    }
}
