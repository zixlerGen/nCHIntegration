
using nCHIntegration.Models;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Globalization;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace nCHIntegration.DIP
{
    public class DIP_HL7Message
    {
        private readonly Patient _patientData;
        private string _gender = string.Empty;
        private string _messageid = string.Empty;
        private string _messageDateTimeNow = string.Empty;
        public DIP_HL7Message(Patient patient)
        {
            _patientData = patient;
        }
        public string HL7_PatientDemographic()
        {
            string hn = _patientData.HN;
            string firstname = _patientData.FirstName.Substring(0, 1) + ".";
            string lastname = _patientData.LastName.Substring(0, 1) + ".";
            string dob = _patientData.DOB.ToString("yyyyMMdd", new CultureInfo("en-US"));
            string agreement = _patientData.Agreement;
            string location = _patientData.Location;
            string pacs = _patientData.PACS;
            string gender = _patientData.Gender;
            if (gender == "ชาย") _gender = "M";
            else if (gender == "หญิง") _gender = "F";

            //ADT Patient demograpice
            _messageid = DateTime.Now.ToString("yyyyMMddHHmmssfff", new CultureInfo("en-US"));
            _messageDateTimeNow = DateTime.Now.ToString("yyyyMMddHHmmss", new CultureInfo("en-US"));
            string hl7Message = @"MSH|^~\&|KKU_V2|KKU|||" + _messageDateTimeNow + "||ADT^A01|" + _messageid + "|C810|2.3||||" + "\r";
            hl7Message += @"EVN|A01|" + _messageDateTimeNow + "|||3455441^SYSTEM^SYSTEM^^^^^^MESSAGING_UNIVMO^PERS^^^MESSAGING" + "\r";
            hl7Message += @"PID|1|"+ _patientData.HN + "^^^KKU^" + _patientData.HN + "|||" + _patientData.LastName.Substring(0, 1) + "." + "^" + _patientData.FirstName.Substring(0, 1) + "." + "||" + _patientData.DOB.ToString("yyyyMMdd", new CultureInfo("en-US")) + "|" + _gender + "|||^^^^^^^||^^^||^^|||||||||||||||" + "\r";
            //hl7Message += @"PV1|0001|C|^^^^^||||^^^^^^^^^^^^^^^";
            return hl7Message;
        }
        public string HL7_PatientInsurance()
        {
            //string hn = _patientData.HN;
            //string firstname = _patientData.FirstName.Substring(0, 1) + ".";
            //string lastname = _patientData.LastName.Substring(0, 1) + ".";
            string dob = _patientData.DOB.ToString("yyyyMMdd", new CultureInfo("en-US"));
            //string agreement = _patientData.Agreement;
            string location = _patientData.Location;
            string pacs = _patientData.PACS;
            string gender = _patientData.Gender;
            if (gender == "ชาย") _gender = "M";
            else if (gender == "หญิง") _gender = "F";

            //ADT Patient Insurance
            _messageid = DateTime.Now.ToString("yyyyMMddHHmmssfff", new CultureInfo("en-US"));
            _messageDateTimeNow = DateTime.Now.ToString("yyyyMMddHHmmss", new CultureInfo("en-US"));
            string hl7Message = @"MSH|^~\&|KKU_V2|KKU|||" + _messageDateTimeNow + "||ADT^A01|" + _messageid + "|C810|2.3" + "\r";
            hl7Message += @"EVN|A01|" + _messageDateTimeNow + "|||3455441^SYSTEM^SYSTEM^^^^^^MESSAGING_UNIVMO^PERS^^^MESSAGING" + "\r";
            hl7Message += @"PID|1|" + _patientData.HN + "^^^KKU^" + _patientData.HN + "|"+ _patientData.Agreement + "^^^KKUINS^IN||" + _patientData.LastName.Substring(0, 1) + "." + "^" + _patientData.FirstName.Substring(0, 1) + "." + "||" + _patientData.DOB.ToString("yyyyMMdd", new CultureInfo("en-US")) + "|" + _gender + "|||^^^^^^^||^^^||^^|||||||||||||||" + "\r";

            return hl7Message;
        }
        public string HL7_RADPacsURL()
        {
            string hn = _patientData.HN;
            string firstname = _patientData.FirstName.Substring(0, 1) + ".";
            string lastname = _patientData.LastName.Substring(0, 1) + ".";
            string dob = _patientData.DOB.ToString("yyyyMMdd", new CultureInfo("en-US"));
            string agreement = _patientData.Agreement;
            string location = _patientData.Location;
            string pacs = _patientData.PACS;
            string gender = _patientData.Gender;
            if (gender == "ชาย") _gender = "M";
            else if (gender == "หญิง") _gender = "F";

            //ORU Patient PACS URL
            _messageid = DateTime.Now.ToString("yyyyMMddHHmmssfff", new CultureInfo("en-US"));
            _messageDateTimeNow = DateTime.Now.ToString("yyyyMMddHHmmss", new CultureInfo("en-US"));
            string hl7Message = @"MSH|^~\&|KKU_V2|KKU|navify|navify|" + _messageDateTimeNow + "||ORU^R01|" + _messageid + "|R|2.3|||AL|ER" + "\r";
            hl7Message += @"PID|1||" + _patientData.HN + "^^^KKU^" + _patientData.HN + "||" + _patientData.LastName.Substring(0, 1) + "." + "^" + _patientData.FirstName.Substring(0, 1) + "." + "||" + _patientData.DOB.ToString("yyyyMMdd", new CultureInfo("en-US")) + "000000|" + _gender + "\r";
            hl7Message += "ORC|RE|55818306JBcv^|55818306JBcv^|70515866cv^|CM||||" + _messageDateTimeNow + "\r";
            hl7Message += "OBR|1|55818306JBcv^||PACS^PACS URL|||"+ _messageDateTimeNow + "||0||F|||"+ _messageDateTimeNow + "||||||||"+ _messageDateTimeNow + "||SP|F|||||||||||"+ _messageDateTimeNow + "|0" + "\r";
            hl7Message += "OBX|1|RP|125460^IMAGEN DE TORAX PORTATIL^RUTA|1|" + _patientData.PACS + "^^^^||||";

            return hl7Message;
        }
        public string HL7_PatientSummary()
        {
            _messageid = DateTime.Now.ToString("yyyyMMddHHmmssfff", new CultureInfo("en-US"));
            _messageDateTimeNow = DateTime.Now.ToString("yyyyMMddHHmmss", new CultureInfo("en-US"));
            string hl7Message = @"MSH|^~\&|KKU_V2|KKU|NAVIFY|ROCHE|" + _messageDateTimeNow + "||ORM^O01|" + _messageid + "|P|2.3||||||" + "\r";
            hl7Message += @"PID|1||" + _patientData.HN + "^^^KKU^MRN||" + _patientData.LastName.Substring(0, 1) + "." + "^" + _patientData.FirstName.Substring(0, 1) + "." + "||" + _patientData.DOB.ToString("yyyyMMdd", new CultureInfo("en-US")) + "000000|" + _gender + "\r";
            //hl7Message += @"ORC|NW|" + _messageid + "^" + "\r";
            hl7Message += @"OBR|1|" + _messageid + "^APPOINTMENT" + "\r";
            hl7Message += @"OBX|1|FT|60591-5|APPOINTMENT|สิทธิ: " + _patientData.Agreement + " จังหวัด: " + _patientData.Location + "\r";
            return hl7Message;
        }
        public async Task<List<string>> HL7_PathologyResult()
        {
            List<string> lstHL7String = new List<string>();

            foreach (var item in _patientData.PathologyResults)
            {
                _messageid = DateTime.Now.ToString("yyyyMMddHHmmssfff", new CultureInfo("en-US"));
                _messageDateTimeNow = DateTime.Now.ToString("yyyyMMddHHmmss", new CultureInfo("en-US"));
                _messageDateTimeNow = DateTime.Now.ToString("yyyyMMddHHmmss", new CultureInfo("en-US"));
                string hl7Message = @"MSH|^~\&|KKU_V2|KKU|navify|navify|" + _messageDateTimeNow + "||ORU^R01|" + _messageid + "|R|2.3|||AL|ER" + "\r";
                hl7Message += @"PID|1||" + _patientData.HN + "^^^KKU^MRN||" + _patientData.FirstName.Substring(0, 1) + "^" + _patientData.LastName.Substring(0, 1) + "||" + _patientData.DOB.ToString("yyyyMMdd", new CultureInfo("en-US")) + "000000|" + _gender + "\r";
                //hl7Message += @"PV1||||||||||||||||||||||||||||||||||||||||||||||" + "\r";
                

                string requestdate = Convert.ToDateTime(item.RequestDate).ToString("yyyyMMddHHmmss", new CultureInfo("en-US"));
                string resultdate = Convert.ToDateTime(item.ResultDate).ToString("yyyyMMddHHmmss", new CultureInfo("en-US"));
                string resultvalue = item.ResultValue;
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PDF"));
                string pdfFileName = $"HL7_PATH_ORU_R01_{resultdate}_{_patientData.HN}_{DateTime.Now:yyyyMMddHHmmssfff}.pdf";
                string pdfFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PDF", pdfFileName);
                string pdfBase64Text = await ConvertRtfToPlainTextAndPdfAsync(resultvalue, pdfFilePath);

                hl7Message += @"ORC|RE|" + item.RequestNumber + "^|" + item.RequestNumber + "^|" + item.RequestItemCode + "^|CM||||" + requestdate + "\r";
                hl7Message += @"OBR|1|" + item.RequestNumber + "^||MRGH^"+ item.RequestItemName + "|||" + requestdate + "||0||F|||" + requestdate + "||||||||" + requestdate + "||PAT|F|||||||||||" + resultdate + "|0" + "\r";
                //hl7Message += @"OBX|1|RP|" + item.RequestItemCode + "^" + item.RequestItemName + "^^^|1|" + pdfFileName + "|^^^^|||0||||||^^^^|^^^|^^^^|||" + "\r";
                //hl7Message += @"OBR|2|" + item.RequestNumber + "^HO||^^^^|||" + requestdate + "||0|^^^|F|||" + requestdate + "||||||||" + requestdate + "||SP|F|||||||||||" + resultdate + "|0|||" + "\r";
                hl7Message += @"OBX|1|ED|125460^Pathology Report^|1|^^PDF^BASE64^" + pdfBase64Text + "||||";

                lstHL7String.Add(hl7Message);
            }


            return lstHL7String;
        }
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
            //plainText = HtmlToPlainText(plainText);
            //CreatePdfFromPlainText(plainText, outputPdfPath);
            //await Task.Delay(1);

            await GeneratePdfFromHtml(plainText, outputPdfPath);

            //await ConvertHtmlToPdfAsync(plainText, outputPdfPath);
            // Step 3: Return the plain text
            string pdfPlainText = ConvertPdfToBase64(outputPdfPath);

            return pdfPlainText;
        }

        public static string HtmlToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // ดึงเฉพาะข้อความ (รวมข้อความใน child nodes)
            string text = doc.DocumentNode.InnerText;

            // ลบช่องว่างซ้ำ
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

            return text;
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
    }
}
