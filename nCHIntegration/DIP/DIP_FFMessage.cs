using HtmlAgilityPack;
using nCHIntegration.Models;
using nCHIntegration.ViewModels;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using RtfPipe.Tokens;
using System.Globalization;
using System.Net.Mail;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nCHIntegration.DIP
{
    public class DIP_FFMessage
    {
        private readonly Patient _patientData;
        private string _gender = string.Empty;
        //private string _messageid = string.Empty;
        private string _messageDateTimeNow = string.Empty;
        private string _messageDateTime = string.Empty;
        public DIP_FFMessage(Patient patient)
        {
            _patientData = patient;
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
        public string FF_PatientDemographic()
        {
            string gender = _patientData.Gender;
            if (gender == "ชาย") _gender = "male";
            else if (gender == "หญิง") _gender = "female";

            string FFMessage = @"SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PrimaryIdentifier,PrimaryIdentifierSystem,LastName,FirstName,BirthDate,GenderCode" + "\r";
            FFMessage += @"KKU,KKU_FLATFILE,nCH," + _patientData.HN + ",KKU," + _patientData.LastName.Substring(0, 1) + ".," + _patientData.FirstName.Substring(0, 1) + ".," + _patientData.DOB.ToString("yyyy-MM-dd", new CultureInfo("en-US")) + "," + _gender;
            return FFMessage;
        }
        public string FF_DiagnosticReportImage()
        {
            _messageDateTimeNow = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss+07:00", new CultureInfo("en-US"));
            string FFMessage = @"ResourceType,SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PatientPrimaryIdentifier,PatientPrimaryIdentifierSystem,DiagnosticPrimaryIdentifier,DiagnosticPrimaryIdentifierSystem,PrimaryReportStatus,DiagnosticReportCode,DiagnosticReportCodeSystem,DiagnosticReportDisplay,EffectiveDateTime,Issued,DiagnosisCategory,DiagnosisCategorySystem,DiagnosisCategoryDisplay,MediaAttachmentURL,MediaAttachmentContent,MediaAttachmentContentMimeType,MediaFileName" + "\r";
            FFMessage += @"DiagnosticReport,KKU,KKU_FLATFILE,nCH," + _patientData.HN + ",KKU," + _patientData.HN + ",LONC,final,100012-4,LOINC,PACS URL," + _messageDateTimeNow + "," + _messageDateTimeNow + ",RAD,,Radiology," + _patientData.PACS + ",dummycontent,image/jpeg,testfilename";
            return FFMessage;
        }

        public string FF_DocumentReference()
        {
            _messageDateTimeNow = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss+07:00", new CultureInfo("en-US"));
            _messageDateTime = DateTime.Now.ToString("yyyy-MM-dd", new CultureInfo("en-US"));
            string patientInsurance = @"Updateสิทธิ: " + _patientData.Agreement + " จังหวัด: " + _patientData.Location;
            patientInsurance = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(patientInsurance));
            string FFMessage = @"ResourceType,SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PatientPrimaryIdentifier,PatientPrimaryIdentifierSystem,DocumentReferenceMasterIdentifier,DocumentReferenceMasterIdentifierSystem,DocumentReferenceIdentifier,DocumentReferenceIdentifierSystem,DocumentReferenceStatus,InlineDocStatus,TypeCode,TypeCodeSystem,TypeDisplay,CategoryCode,CategoryCodeSystem,CategoryCodeDisplay,Date,Description,AttachmentContent,AttachmentContentMimeType" + "\r";
            FFMessage += @"DocumentReference,KKU,KKU_FLATFILE,nCH," + _patientData.HN + ",KKU," + _patientData.HN + "Summ,," + _patientData.HN + "Summ,,current,final,34109-9,LOINC,Patient summary,60591-5,LOINC,Patient summary," + _messageDateTime + ",NTB Summary description,"+ patientInsurance + ",text/plain";
            return FFMessage;
        }

        public async Task<List<string>> FF_DiagnosticPathologyReport()
        {
            List<string> lstFFString = new List<string>();

            foreach (var item in _patientData.PathologyResults)
            {
                string requestdate = Convert.ToDateTime(item.RequestDate).ToString("yyyy-MM-dd", new CultureInfo("en-US"));
                string resultdate = Convert.ToDateTime(item.ResultDate).ToString("yyyy-MM-dd", new CultureInfo("en-US"));
                string resultvalue = item.ResultValue;
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PDF"));
                string pdfFileName = $"FF_Pathology_DiagnosisReport_{resultdate}_{_patientData.HN}_{DateTime.Now:yyyyMMddHHmmssfff}.pdf";
                string pdfFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PDF", pdfFileName);
                string pdfBase64Text = await ConvertRtfToPlainTextAndPdfAsync(resultvalue, pdfFilePath);

                _messageDateTimeNow = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss+07:00", new CultureInfo("en-US"));
                string FFMessage = @"ResourceType,SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PatientPrimaryIdentifier,PatientPrimaryIdentifierSystem,DiagnosticPrimaryIdentifier,DiagnosticPrimaryIdentifierSystem,PrimaryReportStatus,DiagnosticReportText,EffectiveDateTime,DiagnosisCategory,DiagnosisCategorySystem,DiagnosisCategoryDisplay,AttachmentName,AttachmentContent,AttachmentContentMimeType,Issued" + "\r";
                FFMessage += @"DiagnosticReport,KKU,KKU_FLATFILE,nCH," + _patientData.HN + ",KKU," + item.RequestNumber + ",,final, "+ item.RequestItemName + ","+ requestdate + ",PAT,http://terminology.hl7.org/CodeSystem/v2-0074,Pathology,"+ pdfFileName + ","+ pdfBase64Text + ",application/pdf," + resultdate;
                lstFFString.Add(FFMessage);
            }
            return lstFFString;
        }

        public string FF_PatientDemographicINS()
        {
            string gender = _patientData.Gender;
            if (gender == "ชาย") _gender = "male";
            else if (gender == "หญิง") _gender = "female";

            string FFMessage = @"SourceOrgIdentifier,SourceSystemIdentifier,DestinationSystemIdentifier,PrimaryIdentifier,PrimaryIdentifierSystem,SecondaryIdentifier,SecondaryIdentifierSystem,LastName,FirstName,BirthDate,GenderCode" + "\r";
            FFMessage += @"KKU,KKU_FLATFILE,nCH,"+ _patientData.HN + ",KKU,"+ _patientData.Agreement + ",KKUINS,"+ _patientData.LastName.Substring(0, 1) + ".," + _patientData.FirstName.Substring(0, 1) + ".,"+ _patientData.DOB.ToString("yyyy-MM-dd", new CultureInfo("en-US")) + "," + _gender;

            return FFMessage;
        }

    }
}
