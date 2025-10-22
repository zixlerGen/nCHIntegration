using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using nCHIntegration.Data;
using nCHIntegration.DIP;
using nCHIntegration.Models;
using nCHIntegration.ViewModels;
using Renci.SshNet;
using System.Threading.Tasks;

namespace nCHIntegration.Controllers
{
    public class PatientController : Controller
    {
        private readonly AppHODBContext _context;
        //private readonly ILogger<HomeController> _logger;

        public PatientController(AppHODBContext db)
        {
            _context = db;
        }
        [Authorize(Roles = "User")]
        public IActionResult Index(string hn)
        {
            //hn = "IS5656";
            //if (string.IsNullOrEmpty(hn))
            //{
            //    return View(); // Return default view if no search input
            //}

            // Fix for Problem 1: Correct the type to List<PathologyResult> since the query returns PathologyResult
            List<PathologyResult> pathoResult = _context.PathoResult
                .FromSql($"EXEC P_PLOB_nTB_PhatologyResult {hn}")
                .ToList();

            // Fix for Problem 2: Add null-checking to handle possible null values
            Patient? patientRecord = _context.Patients
                .FromSql($"EXEC P_PLOB_nTB_PatientDemographics {hn}")
                .AsEnumerable()
                .FirstOrDefault();

            if (patientRecord == null)
            {
                // Handle the case where no patient record is found
                //return NotFound();
            }
            else
            {
                patientRecord.PathologyResults = pathoResult;
                TempData["HN"] = patientRecord.HN;
            }

            return View(patientRecord);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> SendTonavify(Patient model)
        {
            HL7MessageVM hl7MessageVM = new HL7MessageVM();
            var patient = model;
            List<PathologyResult> pathoResult = _context.PathoResult
                .FromSql($"EXEC P_PLOB_nTB_PhatologyResult {patient.HN}")
                .ToList();

            // Fix for Problem 2: Add null-checking to handle possible null values
            Patient? patientRecord = _context.Patients
                .FromSql($"EXEC P_PLOB_nTB_PatientDemographics {patient.HN}")
                .AsEnumerable()
                .FirstOrDefault();

            if (patientRecord != null)
            {
                patientRecord.PathologyResults = pathoResult;
                DIP_HL7Message hl7Message = new DIP_HL7Message(patientRecord);

                #region Generate HL7 file
                string hl7MessageFormat = hl7Message.HL7_PatientDemographic();
                HL7Message result = WriteDataToFile(hl7MessageFormat, patientRecord.HN, "PatientDemographic", "ADT");
                hl7MessageVM.hl7Message.Add(result);

                //hl7MessageFormat = hl7Message.HL7_PatientInsurance();
                //result = WriteDataToFile(hl7MessageFormat, patientRecord.HN, "PatientInsurance", "ADT");
                //hl7MessageVM.hl7Message.Add(result);

                hl7MessageFormat = hl7Message.HL7_RADPacsURL();
                result = WriteDataToFile(hl7MessageFormat, patientRecord.HN, "PACSURL", "ORU");
                hl7MessageVM.hl7Message.Add(result);

                hl7MessageFormat = hl7Message.HL7_PatientSummary();
                result = WriteDataToFile(hl7MessageFormat, patientRecord.HN, "PatientSummary", "ORM");
                hl7MessageVM.hl7Message.Add(result);

                hl7MessageVM.hl7ORU = await hl7Message.HL7_PathologyResult();
                foreach (var item in hl7MessageVM.hl7ORU)
                {
                    result = WriteDataToFile(item, patientRecord.HN, "PathologyResult", "ORU");
                    hl7MessageVM.hl7Message.Add(result);
                }
                #endregion

                #region Generate FlatFile
                //DIP_FFMessage fFMessage = new DIP_FFMessage(patientRecord);
                //string FFMessageFormat = fFMessage.FF_PatientDemographic();
                //HL7Message resultFF = WriteDataToFlatFile(FFMessageFormat, patientRecord.HN, "PatientDemographic", "01_patient_flatfile");
                //hl7MessageVM.hl7Message.Add(resultFF);

                //FFMessageFormat = fFMessage.FF_DiagnosticReportImage();
                //resultFF = WriteDataToFlatFile(FFMessageFormat, patientRecord.HN, "PACSURL", "_flatfile_diagnosticreport");
                //hl7MessageVM.hl7Message.Add(resultFF);

                //FFMessageFormat = fFMessage.FF_DocumentReference();
                //resultFF = WriteDataToFlatFile(FFMessageFormat, patientRecord.HN, "PatientInsurance", "_flatfile_documentreference");
                //hl7MessageVM.hl7Message.Add(resultFF);

                //FFMessageFormat = fFMessage.FF_PatientDemographicINS();
                //resultFF = WriteDataToFlatFile(FFMessageFormat, patientRecord.HN, "PatientInsurance", "02_patient_flatfile");
                //hl7MessageVM.hl7Message.Add(resultFF);

                //hl7MessageVM.hl7ORU = await fFMessage.FF_DiagnosticPathologyReport();
                //int FileSeq = 0;
                //foreach (var item in hl7MessageVM.hl7ORU)
                //{
                //    FileSeq++;
                //    resultFF = WriteDataToFlatFile(item, patientRecord.HN, "PathologyResult", FileSeq + "_PAT_flatfile_diagnosticreport");
                //    hl7MessageVM.hl7Message.Add(resultFF);
                //}
                #endregion
            }
            return View("SendTonavify", hl7MessageVM);
        }

        private HL7Message WriteDataToFile(string hl7MessageContent, string hn, string messaeName, string messageType)
        {
            HL7Message hl7 = new HL7Message();
            try
            {
                hl7.MessageType = messageType;
                hl7.MessageName = messaeName;
                hl7.MessageContent = hl7MessageContent;
                hl7.MessageID = "";
                hl7.SFTPSendOutTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                hl7.SFTPSendOutStatus = "Success";

                // Write HL7 messages to a text file
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "HL7"));
                string fileName = $"HL7_{hn}_{messaeName}_{DateTime.Now:yyyyMMddHHmmssfff}.hl7";
                hl7.FilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "HL7", fileName);

                // Write the data to the file (overwrites if the file exists)
                System.IO.File.WriteAllText(hl7.FilePath, hl7MessageContent);
                return hl7;
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Console.WriteLine($"An error occurred while writing to the file: {ex.Message}");
                return hl7;
            }
        }
        private HL7Message WriteDataToFlatFile(string hl7MessageContent, string hn, string messaeName, string messageType)
        {
            HL7Message hl7 = new HL7Message();
            try
            {
                hl7.MessageType = messageType;
                hl7.MessageName = messaeName;
                hl7.MessageContent = hl7MessageContent;
                hl7.MessageID = "";
                hl7.SFTPSendOutTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                hl7.SFTPSendOutStatus = "Success";

                // Write HL7 messages to a text file
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FF"));
                string fileName = $"{messageType}.csv";
                hl7.FilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FF", fileName);

                // Write the data to the file (overwrites if the file exists)
                System.IO.File.WriteAllText(hl7.FilePath, hl7MessageContent);
                return hl7;
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Console.WriteLine($"An error occurred while writing to the file: {ex.Message}");
                return hl7;
            }
        }
        private HL7Message SendFileViaSFTPKey(HL7Message hl7Message)
        {
            string host = "sftp.example.com"; // SFTP server
            int port = 22; // Default SFTP port
            string username = "your_username";
            string privateKeyPath = @"C:\path\to\your\privatekey.pem"; // Path to your private key file
            string remotePath = "/remote/directory/HL7Messages.txt"; // Remote file path

            if (string.IsNullOrEmpty(hl7Message.FilePath))
            {
                hl7Message.SFTPSendOutStatus = "FilePath is null or empty. Cannot proceed with SFTP upload";
                hl7Message.SFTPSendOutTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                return hl7Message;
            }

            // Load the private key
            var privateKey = new Renci.SshNet.PrivateKeyFile(privateKeyPath);
            var authMethod = new Renci.SshNet.PrivateKeyAuthenticationMethod(username, privateKey);
            var connectionInfo = new Renci.SshNet.ConnectionInfo(host, port, username, authMethod);

            using (var client = new Renci.SshNet.SftpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    using (var fileStream = new FileStream(hl7Message.FilePath, FileMode.Open))
                    {
                        client.UploadFile(fileStream, remotePath);
                    }
                    client.Disconnect();
                    hl7Message.SFTPSendOutStatus = "Success";
                    hl7Message.SFTPSendOutTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    return hl7Message;
                }
                catch (Exception ex)
                {
                    string error = $"An error occurred while sending the file via SFTP: {ex.Message}";
                    hl7Message.SFTPSendOutStatus = error;
                    hl7Message.SFTPSendOutTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    return hl7Message;
                }
            }
        }
        private HL7Message SendFileViaSFTP(HL7Message hl7Message)
        {
            string host = "sftp.example.com"; // Replace with your SFTP server
            int port = 22; // Default SFTP port
            string username = "your_username";
            string password = "your_password";
            string remotePath = "/remote/directory/HL7Messages.txt"; // Remote file path

            if (string.IsNullOrEmpty(hl7Message.FilePath))
            {
                //Console.WriteLine("FilePath is null or empty. Cannot proceed with SFTP upload.");
                //return "FilePath is null or empty.";
                hl7Message.SFTPSendOutStatus = "FilePath is null or empty. Cannot proceed with SFTP upload'";
                hl7Message.SFTPSendOutTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                return hl7Message;
            }

            using (var client = new Renci.SshNet.SftpClient(host, port, username, password))
            {
                try
                {
                    client.Connect();
                    using (var fileStream = new FileStream(hl7Message.FilePath, FileMode.Open))
                    {
                        client.UploadFile(fileStream, remotePath);
                    }
                    client.Disconnect();
                    hl7Message.SFTPSendOutStatus = "Success";
                    hl7Message.SFTPSendOutTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    return hl7Message;
                }
                catch (Exception ex)
                {
                    string error = ($"An error occurred while sending the file via SFTP: {ex.Message}");
                    hl7Message.SFTPSendOutStatus = error;
                    hl7Message.SFTPSendOutTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    return hl7Message;
                }
            }
        }
    }
}
