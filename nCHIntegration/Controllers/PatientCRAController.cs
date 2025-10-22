using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using nCHIntegration.Data;
using nCHIntegration.DIP;
using nCHIntegration.Models;
using nCHIntegration.Utilities;
using nCHIntegration.ViewModels;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Tls;
using Renci.SshNet.Messages.Connection;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace nCHIntegration.Controllers
{
    public class PatientCRAController : Controller
    {
        private readonly string _connectionString;
        private readonly AppDBContext _dbContext;
        private readonly string examHN = "";
        private readonly ILogger<PatientCRAController> _logger;

        private readonly IConverter _converter;
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public PatientCRAController(IConfiguration configuration, AppDBContext dbContext, IConverter converter,
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider, ILogger<PatientCRAController> logger)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _connectionString = configuration.GetConnectionString("CRAConnectionString")
                                ?? throw new InvalidOperationException("CRAConnectionString is not configured.");
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            _converter = converter;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(DateTime mdtDate)
        {
            //List<CRA_PatientAppointment> appointments = await GetDataFromMySQL(mdtDate);
            List<CRA_MDT_Appointment> appointmentsMDT = await GetDataFromMySQL(mdtDate);
            List<CRA_MDT_Consult> consultMDT = await GetMDTDoctorConsult(mdtDate);
            //List<CRA_MDT_Appointment> appointmentsMDT = await GetMDTAppointmentsFromSqlServer(mdtDate);
            List<CRA_Patient_Right> patientRights = await GetPatientRights();
            List<CRA_MDT_Group> mdtGroup = await GetCRAMDTGroup();
            List<CRA_MDT_Doctor> mdtDoctorList = await GetCRAMDTDoctorList();
            List<CRA_Point_Of_Discussion> point_Of_Discussions = await GetPointOfDiscussion();
            //return View(appointmentsMDT);
            return View(Tuple.Create(appointmentsMDT, patientRights, mdtGroup, mdtDoctorList, consultMDT, point_Of_Discussions));
        }
        private async Task<List<CRA_MDT_Appointment>> GetDataFromMySQL(DateTime mdtDate)
        {
            //List<CRA_PatientAppointment> ssbAppointmentsList = new List<CRA_PatientAppointment>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"SELECT * FROM tb_patient_appointment WHERE DATE(appoint_datetime) = @MDTDate";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MDTDate", mdtDate);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            CRA_PatientAppointment appointment = new CRA_PatientAppointment
                            {
                                // String Columns: ตรวจสอบ NULL ก่อนอ่าน
                                HN = reader.IsDBNull("HN") ? null : reader.GetString("HN"),
                                Appoint_No = reader.IsDBNull("Appoint_No") ? null : reader.GetString("Appoint_No"),
                                Clinic_Code = reader.IsDBNull("Clinic_Code") ? null : reader.GetString("Clinic_Code"),
                                Clinic_Name = reader.IsDBNull("Clinic_Name") ? null : reader.GetString("Clinic_Name"),
                                Initial_Name_Code = reader.IsDBNull("Initial_Name_Code") ? null : reader.GetString("Initial_Name_Code"),
                                Initial_Name = reader.IsDBNull("Initial_Name") ? null : reader.GetString("Initial_Name"),
                                First_Name = reader.IsDBNull("First_Name") ? null : reader.GetString("First_Name"),
                                Last_Name = reader.IsDBNull("Last_Name") ? null : reader.GetString("Last_Name"),
                                Gender_Name = reader.IsDBNull("Gender_Name") ? null : reader.GetString("Gender_Name"),
                                Right_Code = reader.IsDBNull("Right_Code") ? null : reader.GetString("Right_Code"),
                                Right_Name = reader.IsDBNull("Right_Name") ? null : reader.GetString("Right_Name"),
                                Address1 = reader.IsDBNull("Address1") ? null : reader.GetString("Address1"),
                                Province = reader.IsDBNull("Province") ? null : reader.GetString("Province"),
                                Province_Name = reader.IsDBNull("Province_Name") ? null : reader.GetString("Province_Name"),
                                Amphoe = reader.IsDBNull("Amphoe") ? null : reader.GetString("Amphoe"),
                                Amphoe_Name = reader.IsDBNull("Amphoe_Name") ? null : reader.GetString("Amphoe_Name"),
                                Tambon = reader.IsDBNull("Tambon") ? null : reader.GetString("Tambon"),
                                Tambon_Name = reader.IsDBNull("Tambon_Name") ? null : reader.GetString("Tambon_Name"),
                                Postal_Code = reader.IsDBNull("Postal_Code") ? null : reader.GetString("Postal_Code"),
                                Doctor = reader.IsDBNull("Doctor") ? null : reader.GetString("Doctor"),
                                Doctor_Name = reader.IsDBNull("Doctor_Name") ? null : reader.GetString("Doctor_Name"),
                                CTB_No = reader.IsDBNull("CTB_No") ? null : reader.GetString("CTB_No"),
                                Remarks_Memo = reader.IsDBNull("Remarks_Memo") ? null : reader.GetString("Remarks_Memo"),
                                appoint_by_doctor = reader.IsDBNull("appoint_by_doctor") ? null : reader.GetString("appoint_by_doctor"),
                                appoint_by_doctor_name = reader.IsDBNull("appoint_by_doctor_name") ? null : reader.GetString("appoint_by_doctor_name"),
                                Update_By = reader.IsDBNull("Update_By") ? null : reader.GetString("Update_By"),

                                // Integer/DateTime Columns: ต้องตรวจสอบด้วยว่าคุณสมบัติใน Model เป็น nullable หรือไม่ (เช่น int? หรือ DateTime?)
                                Gender = reader.IsDBNull("Gender") ? 0 : reader.GetInt32("Gender"), // ถ้า Gender เป็น int ธรรมดา
                                Appoint_Datetime = reader.GetDateTime("Appoint_Datetime"), // สมมติว่าไม่เป็น NULL
                                Birth_Datetime = reader.GetDateTime("Birth_Datetime"), // สมมติว่าไม่เป็น NULL
                                Update_Date = reader.GetDateTime("Update_Date"), // สมมติว่าไม่เป็น NULL
                            };
                            //appointments.Add(appointment);
                            _logger.LogInformation("InsertMDTPatientList: Adding new record." + appointment.HN);
                            await InsertMDTPatientList(appointment);
                        }
                    }
                }
            }
            List<CRA_MDT_Appointment> appointmentsMDTList = await GetMDTAppointmentsFromSqlServer(mdtDate);
            return appointmentsMDTList;
        }
        private async Task<CRA_MDT_Appointment> MapAppointmentToMDTPatientList(CRA_PatientAppointment appointment)
        {
            // Determine MDT_Meeting_Name based on Appoint_Datetime
            string mdtMeetingName = null;
            var dayOfWeek = appointment.Appoint_Datetime.DayOfWeek;
            if (dayOfWeek == DayOfWeek.Thursday)
                mdtMeetingName = "Breast";
            else if (dayOfWeek == DayOfWeek.Friday)
                mdtMeetingName = "Pan";

            return new CRA_MDT_Appointment
            {
                HN = appointment.HN,
                Appoint_Datetime = appointment.Appoint_Datetime,
                Initial_Name = appointment.Initial_Name,
                First_Name = appointment.First_Name,
                Last_Name = appointment.Last_Name,
                Gender_Name = appointment.Gender_Name,
                Birth_Datetime = appointment.Birth_Datetime,
                Right_Code = appointment.Right_Code,
                Right_Name = appointment.Right_Name,
                Province_Name = appointment.Province_Name,
                Doctor_Name = appointment.Doctor_Name,
                CTB_No = appointment.CTB_No,
                Remarks_Memo = appointment.Remarks_Memo,
                Confirm_Date = null,
                Confirm_By = null,
                Generate_File = false,
                MDT_Group = null,
                MDT_Visit = null,
                MDT_Consult = null,
                MDT_Review_Image = null,
                MDT_Appointment = null,
                Update_Date = null,
                Update_By = null,
                Confirm_Status = false,
                Imaging_Status = false,
                Patho_Status = false,
                History_Status = false,
                appoint_by_doctor = appointment.appoint_by_doctor,
                appoint_by_doctor_name = appointment.appoint_by_doctor_name,
                Diagnostic_Name = await GetTbPatientDiagnosisFirstRow(appointment.HN),
                Point_Of_Discussion = null,
                MDT_Meeting_Name = mdtMeetingName
            };
        }
        public async Task<IActionResult> InsertMDTPatientList(CRA_PatientAppointment appointment)
        {
            try
            {
                var record = _dbContext.CRA_MDT_Appointment
                    .FirstOrDefault(a => a.HN == appointment.HN && a.Appoint_Datetime.Date == appointment.Appoint_Datetime);
                _logger.LogInformation("InsertMDTPatientList: Adding new record.");

                if (record == null)
                {
                    CRA_MDT_Appointment mdtPatientList = await MapAppointmentToMDTPatientList(appointment);
                    _dbContext.CRA_MDT_Appointment.Add(mdtPatientList);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("SaveAppointment: SaveChanges successful.");
                }

                return Ok("Data inserted into CRA_MDTPatientList successfully.");
            }
            catch (DbUpdateException dbEx)
            {
                var innerEx = dbEx.InnerException?.Message ?? dbEx.Message;
                _logger.LogError(dbEx, "InsertMDTPatientList: Database Update Failed. Inner Exception: {InnerMessage}", innerEx);
                return Json(new { success = false, message = $"Database Error: {innerEx}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InsertMDTPatientList: General Server Error.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<List<CRA_MDT_Appointment>> GetMDTAppointmentsFromSqlServer(DateTime mdtDate)
        {
            //        var appointments1 = await _dbContext.CRA_MDT_Appointment
            //.Where(a => a.Appoint_Datetime.Date == mdtDate.Date)
            //.Select(a => new { a.HN, a.MDT_Visit })  // Check specific fields
            //.ToListAsync();

            //        var appointmentstest = _dbContext.CRA_MDT_Appointment
            //            .Where(a => a.Appoint_Datetime.Date == mdtDate.Date);

            //        var sql2 = appointmentstest.ToQueryString();


            var appointments = await _dbContext.CRA_MDT_Appointment
                .AsNoTracking()
                .Where(a => a.Appoint_Datetime.Date == mdtDate.Date)
                .ToListAsync();

            return appointments;
        }
        [HttpGet]
        public async Task<List<CRA_MDT_Consult>> GetMDTDoctorConsult(DateTime mdtDate)
        {
            var consult = await _dbContext.CRA_MDT_Consult
                .AsNoTracking()
                .Where(a => a.Appoint_Datetime.Date == mdtDate.Date)
                .ToListAsync();

            return consult;
        }
        [HttpGet]
        public async Task<List<CRA_Patient_Right>> GetPatientRights()
        {
            var patientrights = await _dbContext.CRA_Patient_Right.ToListAsync();

            return patientrights;
        }
        [HttpGet]
        public async Task<List<CRA_MDT_Group>> GetCRAMDTGroup()
        {
            var mdtGroup = await _dbContext.CRA_MDT_Group
                .OrderBy(m => m.MDTGroup_Name)
                .ToListAsync();

            return mdtGroup;
        }
        [HttpGet]
        public async Task<List<CRA_Point_Of_Discussion>> GetPointOfDiscussion()
        {
            var pointOfDiscussion = await _dbContext.CRA_Point_Of_Discussion
                .OrderBy(m => m.Point_Of_Discussion)
                .ToListAsync();

            return pointOfDiscussion;
        }
        [HttpGet]
        public async Task<List<CRA_MDT_Doctor>> GetCRAMDTDoctorList()
        {
            var mdtDoctorList = await _dbContext.CRA_MDT_Doctor
                .OrderBy(d => d.Doctor_Firstname)
                .ToListAsync();

            return mdtDoctorList;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrEdit(IFormCollection form)
        {
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
            {
                return BadRequest("This endpoint only accepts AJAX requests");
            }
            try
            {
                string hn = form["HN"].ToString() ?? string.Empty;
                bool isSendToNch = bool.Parse(form["isSendToNch"].ToString() ?? "false");
                DateTime mdtDate = Convert.ToDateTime(form["MDTDate"]);

                // Find the record in the database by HN and Appoint_Datetime.Date == mdtDate.Date
                var record = _dbContext.CRA_MDT_Appointment
                    .FirstOrDefault(a => a.HN == hn && a.Appoint_Datetime.Date == mdtDate.Date);

                if (record != null)
                {
                    // Update fields
                    record.CTB_No = form["CTB_No"].ToString();
                    record.MDT_Visit = int.TryParse(form["MDT_Visit"].ToString(), out int visitVal) ? visitVal : null;
                    record.Remarks_Memo = form["Remarks_Memo"].ToString();
                    record.Doctor_Name = form["DoctorList"].ToString();
                    record.Right_Name = form["Patient_Rights"].ToString();
                    record.MDT_Group = form["MDT_Group"].ToString();
                    record.MDT_Consult = form["consult"].ToString();
                    record.MDT_Review_Image = form["reviewimage"].ToString();
                    record.MDT_Appointment = form["appointment"].ToString();
                    record.Update_By = "System";
                    record.Update_Date = DateTime.Now;
                    record.Diagnostic_Name = form["diagnostics"].ToString();
                    record.Imaging_Status = form["imagingstatus"].ToString() == "true";
                    record.Patho_Status = form["pathostatus"].ToString() == "true";
                    record.History_Status = form["historystatus"].ToString() == "true";
                    record.Patient_Right_Group = form["Patient_Rights_Group"].ToString();
                    record.Point_Of_Discussion = form["Point_of_Discussion"].ToString();
                    if (isSendToNch)
                    {
                        record.Confirm_Status = true;
                        record.Confirm_Date = DateTime.Now;
                        // Add any additional logic needed for sending to nCH
                    }

                    await _dbContext.SaveChangesAsync();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Record not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmOrEdit3(CRA_MDT_Appointment model)
        {
            if (ModelState.IsValid)
            {

                _dbContext.Update(model); // หรือ _context.Add(model) ถ้าเป็นการเพิ่มใหม่
                await _dbContext.SaveChangesAsync();
                return Ok();
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmOrEdit2([FromForm] CRA_MDT_Appointment model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var existingAppointment = await _dbContext.CRA_MDT_Appointment
                    .FirstOrDefaultAsync(a => a.HN == model.HN && a.Appoint_Datetime.Date == model.Appoint_Datetime.Date);

                if (existingAppointment != null)
                {
                    // Update only the fields that were present in the form
                    existingAppointment.CTB_No = model.CTB_No;
                    existingAppointment.MDT_Visit = model.MDT_Visit;
                    existingAppointment.Remarks_Memo = model.Remarks_Memo;
                    existingAppointment.Right_Name = model.Right_Name;
                    existingAppointment.MDT_Group = model.MDT_Group;
                    existingAppointment.MDT_Consult = model.MDT_Consult;
                    existingAppointment.MDT_Review_Image = model.MDT_Review_Image;
                    existingAppointment.MDT_Appointment = model.MDT_Appointment;
                    existingAppointment.Update_By = "System";
                    existingAppointment.Update_Date = DateTime.Now;

                    _dbContext.Update(existingAppointment);
                }

                await _dbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> GenerateNavifyFile([FromBody] GenerateNavifyRequest request)
        {
            try
            {
                if (request?.SelectedPatients == null || !request.SelectedPatients.Any())
                {
                    return Json(new { success = false, message = "No patients selected" });
                }

                // 1. Prepare the data from your in-memory collection first.
                //    This step runs in memory, NOT on the database.
                // Extract all Hn values from your selected patients
                var selectedPatientHns = request.SelectedPatients
                    .Select(p => p.Hn)
                    .ToList(); // ToList() ensures it's an in-memory list

                // Extract and parse the MdtDate values.

                var selectedPatientAppointmentDates = request.SelectedPatients
                    .Select(p => DateTime.Parse(p.MdtDate).Date) // Parse the string to DateTime, then get the Date part
                    .Distinct() // Get unique dates to optimize the 'Contains' check
                    .ToList(); // ToList() ensures it's an in-memory list

                // 2. Now, construct a LINQ to Entities query that EF Core CAN translate to SQL.
                //    Assuming 'dbContext' is your instance of DbContext.
                var selectedAppointments = _dbContext.CRA_MDT_Appointment
                    .Where(appointment =>
                        selectedPatientHns.Contains(appointment.HN) && // This translates to SQL 'IN' clause
                        selectedPatientAppointmentDates.Contains(appointment.Appoint_Datetime.Date) // This translates to SQL 'IN' clause for dates
                    );

                // 3. Finally, execute the Any() check on the translatable query.
                var anyMatchingAppointment = selectedAppointments.Any(); // This will execute a COUNT(*) or similar SQL query

                if (!anyMatchingAppointment)
                {
                    return Json(new { success = false, message = "No matching appointments found" });
                }

                #region Generate FlatFile
                foreach (var item in selectedAppointments)
                {
                    CRA_DIP_FFMessage fFMessage = new CRA_DIP_FFMessage(item);
                    string FFMessageFormat = fFMessage.CRA_FF_PatientDemographic();
                    string fileName = $"{DateTime.Now:yyyyMMdd_HHmmssfff}_{item.HN}_CLB_flatfile.csv";
                    HL7Message resultFF = WriteDataToFlatFile(FFMessageFormat, item.HN, "PatientDemographic", fileName);

                    ////Appointment
                    //string appointment = fFMessage.CRA_FF_Appointment("Lung", "consult Med Onco", "consult oncomed neoadjuvant therapy (พญ.อัจฉรา)", "กชพร", "นามสกุล", "394592007", "Clinical oncology");
                    //fileName = $"03_{item.HN}_CLB_{DateTime.Now:yyyyMMdd_HHmmss}_flatfile_appointment.csv";
                    //resultFF = WriteDataToFlatFile(appointment, item.HN, "appointment", fileName);

                    //Appointment Comment
                    string doctorName = item.appoint_by_doctor_name;
                    var nameParts = doctorName.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    string appointmentComment = fFMessage.CRA_FF_AppointmentComments(item.MDT_Meeting_Name, item.Point_Of_Discussion, item.MDT_Consult, nameParts[0],
                        nameParts[1], "394592007", "Clinical oncology", item.MDT_Consult, item.MDT_Consult);
                    fileName = $"{DateTime.Now:yyyyMMdd_HHmmssfff}_17_{item.HN}_CLB_flatfile_appointment.csv";
                    resultFF = WriteDataToFlatFile(appointmentComment, item.HN, "appointmentComment", fileName);

                    //fileName = $"{item.HN}_CLB_{DateTime.Now:yyyyMMdd_HHmmss}_flatfile_appointment.csv";
                    //resultFF = WriteDataToFlatFile(appointmentComment, item.HN, "appointmentComment", fileName);

                    //Diagnostics
                    string diagnosticcondition = fFMessage.CRA_FF_Condition(item.Diagnostic_Name, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"));
                    fileName = $"{DateTime.Now:yyyyMMdd_HHmmssfff}_{item.HN}_CLB_flatfile_condition.csv";
                    resultFF = WriteDataToFlatFile(diagnosticcondition, item.HN, "diagnosticCondition", fileName);

                    //Patient Rights
                    string documentRefernce = fFMessage.CRA_FF_DocumentReference();
                    fileName = $"{DateTime.Now:yyyyMMdd_HHmmssfff}_{item.HN}_CLB_flatfile_documentreference.csv";
                    resultFF = WriteDataToFlatFile(documentRefernce, item.HN, "documentreference", fileName);

                    //Patho Report
                    var selectedPathoResult = _dbContext.CRA_Temp_Patient_Pathology
                        .Where(patho => selectedPatientHns.Contains(patho.HN))
                        .ToList();
                    int pathoFileNo = 1;
                    foreach (var itemPatho in selectedPathoResult)
                    {
                        string FFPathoReport = fFMessage.CRA_FF_PATDiagnosticReport(
                            itemPatho.rich_text_memo,
                            itemPatho.specimen_receive_datetime.HasValue ? itemPatho.specimen_receive_datetime.Value.ToString("yyyy-MM-dd") : "",
                            itemPatho.request_no,
                            itemPatho.path_no,
                            itemPatho.lab_name_english
                        );
                        fileName = $"{DateTime.Now:yyyyMMdd_HHmmssfff}_{pathoFileNo}_{item.HN}_CLB_pat_flatfile_diagnosticreport.csv";
                        resultFF = WriteDataToFlatFile(FFPathoReport, item.HN, "pathologyreport", fileName);
                        pathoFileNo++;
                    }

                    //RAD Report
                    var selectedImageResult = _dbContext.CRA_Temp_Patient_xRay
                        .Where(xRay => selectedPatientHns.Contains(xRay.HN))
                        .ToList();
                    int xrayFileNo = 1;

                    foreach (var itemXray in selectedImageResult)
                    {
                        string FFRADReport = fFMessage.CRA_FF_RADDiagnosticReport(
                            itemXray.Rich_Text_Memo,
                            itemXray.Xray_Date.HasValue ? itemXray.Xray_Date.Value.ToString("yyyy-MM-dd") : "",
                            itemXray.Request_No,
                            itemXray.xray_name_english
                        );
                        fileName = $"{DateTime.Now:yyyyMMdd_HHmmssfff}_{xrayFileNo}_{item.HN}_CLB_rad_flatfile_diagnosticreport.csv";
                        resultFF = WriteDataToFlatFile(FFRADReport, item.HN, "radiologyreport", fileName);
                        xrayFileNo++;
                    }

                    ////Physical Exam
                    //string FFPhysicalExam = fFMessage.CRA_FF_Observation_PhysicalExam("2025-04-18", "Temperature", "37.2 องศาเซลเซียส");
                    //fileName = $"{item.HN}_CLB_{DateTime.Now:yyyyMMdd_HHmmss}_flatfile_observation.csv";
                    //resultFF = WriteDataToFlatFile(FFPhysicalExam, item.HN, "physicalExamination", fileName);

                    //LabBio
                    var selectedLabResult = _dbContext.CRA_Temp_Patient_Lab
                        .Where(lab => selectedPatientHns.Contains(lab.HN))
                        .ToList();
                    int labFileNo = 1;
                    foreach (var itemLab in selectedLabResult)
                    {
                        string FFLabBio = fFMessage.CRA_FF_LabBioObservation(
                            itemLab.request_no + item.HN,
                            itemLab.lab_name_english,
                            itemLab.result_value, 
                            "", 
                            "", 
                            "", 
                            "", 
                            itemLab.result_datetime.HasValue ? itemLab.result_datetime.Value.ToString("yyyy-MM-dd") : ""
                        );
                        fileName = $"{DateTime.Now:yyyyMMdd_HHmmssfff}_{labFileNo}_{item.HN}_CLB_labbio_flatfile_observation.csv";
                        resultFF = WriteDataToFlatFile(FFLabBio, item.HN, "LabBio", fileName);
                        labFileNo++;
                    }

                    ////Patient history
                    var selectedPateintHistory = _dbContext.CRA_Temp_Patient_History
                        .Where(history => selectedPatientHns.Contains(history.HN))
                        .ToList();
                    foreach (var itemHistory in selectedPateintHistory)
                    {
                        string FFPatientHistory = fFMessage.CRA_FF_HistoryOfIllness(
                            itemHistory.Update_Date.HasValue ? itemHistory.Update_Date.Value.ToString("yyyy-MM-dd") : "", 
                            itemHistory.patient_history, 
                            "System"
                        );
                        fileName = $"{DateTime.Now:yyyyMMdd_HHmmssfff}_{item.HN}_CLB_HistoryOfIllness_flatfile_documentreference.csv";
                        resultFF = WriteDataToFlatFile(FFPatientHistory, item.HN, "PatientHistory", fileName);
                    }

                    ////Additional ID
                    if (!string.IsNullOrEmpty(item.CTB_No))
                    {
                        string FFAdditionalID = fFMessage.CRA_FF_PatientAddIdentified(item.CTB_No);
                        fileName = $"{DateTime.Now:yyyyMMdd_HHmmssfff}_CTB_{item.HN}_CLB_patient_flatfile.csv";
                        resultFF = WriteDataToFlatFile(FFAdditionalID, item.HN, "AdditionalID", fileName);
                    }

                    // Update the status
                    item.Confirm_Status = true;
                    item.Confirm_Date = DateTime.Now;
                    item.Confirm_By = "System";
                }
                // Save changes to the database AFTER the loop
                await _dbContext.SaveChangesAsync(); // This will persist all changes made in the loop
                                                     //await GetMDTAppointmentsFromSqlServer(DateTime.Parse(request.SelectedPatients[0].MdtDate).Date);
                #endregion

                // Determine the MDTDate to send back for refresh
                DateTime? mdtDateToRefresh = null;
                if (request.SelectedPatients.Any())
                {
                    mdtDateToRefresh = DateTime.Parse(request.SelectedPatients[0].MdtDate).Date;
                }
                var updatedPatient = request.SelectedPatients.First();
                //return Json(new { success = true, message = "Send to nCH success.", mdtDate = mdtDateToRefresh?.ToString("yyyy-MM-dd") });
                return Json(new
                {
                    success = true,
                    message = "Send HN: " + updatedPatient.Hn + " to nCH success.",
                    hn = updatedPatient.Hn,
                    newStatus = true, // ส่งสถานะใหม่กลับไป
                    confirmDate = DateTime.Now.ToString("yyyy-MM-dd") // หรือรูปแบบที่ต้องการแสดง
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
                //string fileName = $"{messageType}.csv";
                hl7.FilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FF", messageType);

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
        private async Task<List<TbPatientLab>> GetTbPatientLabData(string hn, DateTime? filterDate)
        {
            List<TbPatientLab> CRAPatientLabList = new List<TbPatientLab>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"SELECT * FROM tb_patient_lab WHERE hn = @HN";
                if (filterDate.HasValue)
                {
                    sql = @"SELECT * FROM tb_patient_lab WHERE hn = @HN and DATE(result_datetime) = @filterDate order by result_datetime desc";
                }

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@HN", hn);
                    if (filterDate.HasValue)
                    {
                        command.Parameters.AddWithValue("@filterDate", filterDate.Value.Date);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            TbPatientLab patientLabData = new TbPatientLab
                            {
                                // ID: Use IsDBNull for nullable int fields (assuming ID in C# is int or int?)
                                ID = reader.IsDBNull("ID") ? (int?)null : reader.GetInt32("ID"),

                                // String Fields: Use IsDBNull check for all
                                HN = reader.IsDBNull("HN") ? null : reader.GetString("HN"),
                                Facility_RMS_No = reader.IsDBNull("Facility_RMS_No") ? null : reader.GetString("Facility_RMS_No"),
                                Request_No = reader.IsDBNull("Request_No") ? null : reader.GetString("Request_No"),
                                Lab_Code = reader.IsDBNull("Lab_Code") ? null : reader.GetString("Lab_Code"),
                                Lab_Name_English = reader.IsDBNull("Lab_Name_English") ? null : reader.GetString("Lab_Name_English"),
                                Result_Value = reader.IsDBNull("Result_Value") ? null : reader.GetString("Result_Value"),
                                Lab_Unit = reader.IsDBNull("Lab_Unit") ? null : reader.GetString("Lab_Unit"),
                                Previous_Result_Value = reader.IsDBNull("Previous_Result_Value") ? null : reader.GetString("Previous_Result_Value"),
                                Update_By = reader.IsDBNull("Update_By") ? null : reader.GetString("Update_By"),

                                // DateTime Fields: Check for NULL and cast to nullable DateTime?
                                Result_Datetime = reader.IsDBNull("Result_Datetime")? (DateTime?)null : reader.GetDateTime("Result_Datetime"),

                                Previous_Result_Datetime = reader.IsDBNull("Previous_Result_Datetime")? (DateTime?)null : reader.GetDateTime("Previous_Result_Datetime"),

                                Update_Date = reader.IsDBNull("Update_Date")? (DateTime?)null : reader.GetDateTime("Update_Date")
                            };
                            CRAPatientLabList.Add(patientLabData);
                        }
                    }
                }
            }

            return CRAPatientLabList;
        }
        private async Task<List<TbPatientXray>> GetTbPatientXrayData(string hn, DateTime? filterDate)
        {
            List<TbPatientXray> CRAPatientXrayList = new List<TbPatientXray>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"SELECT * FROM tb_patient_xray WHERE hn = @HN order by date_of_xray desc";

                if (filterDate.HasValue)
                {
                    sql = @"SELECT * FROM tb_patient_xray WHERE hn = @HN AND DATE(date_of_xray) = @filterDate order by date_of_xray desc";
                }

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@HN", hn);
                    if (filterDate.HasValue)
                    {
                        command.Parameters.AddWithValue("@filterDate", filterDate.Value.Date);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            TbPatientXray patientXrayData = new TbPatientXray
                            {
                                // ID: Use IsDBNull for nullable int fields (assuming ID in C# is int or int?)
                                ID = reader.IsDBNull("ID") ? 0 : reader.GetInt32("ID"),

                                // String Fields: Use IsDBNull check for all
                                HN = reader.IsDBNull("HN") ? null : reader.GetString("HN"),
                                Facility_RMS_No = reader.IsDBNull("Facility_RMS_No") ? null : reader.GetString("Facility_RMS_No"),
                                Request_No = reader.IsDBNull("Request_No") ? null : reader.GetString("Request_No"),

                                // DateTime Fields: Use IsDBNull check (assuming C# Model uses DateTime?)
                                Confirm_Result_Datetime = reader.IsDBNull("Confirm_Result_Datetime") ? (DateTime?)null : reader.GetDateTime("Confirm_Result_Datetime"),
                                Date_Of_Xray = reader.IsDBNull("Date_Of_Xray") ? (DateTime?)null : reader.GetDateTime("Date_Of_Xray"),
                                Update_Date = reader.IsDBNull("Update_Date") ? (DateTime?)null : reader.GetDateTime("Update_Date"),

                                // 🛑 RICH_TEXT_MEMO: This is the most likely culprit for the previous error
                                // It must be checked for DBNull BEFORE being passed to RtfConverter.
                                Rich_Text_Memo = reader.IsDBNull("Rich_Text_Memo")
                     ? null // If DB is NULL, assign C# null
                     : RtfConverter.ConvertRtfToPlainTextSimple(reader.GetString("Rich_Text_Memo")),

                                // Remaining String Fields:
                                Update_By = reader.IsDBNull("Update_By") ? null : reader.GetString("Update_By"),
                                xray_code = reader.IsDBNull("xray_code") ? null : reader.GetString("xray_code"),
                                xray_name_english = reader.IsDBNull("xray_name_english") ? null : reader.GetString("xray_name_english")
                            };
                            CRAPatientXrayList.Add(patientXrayData);
                        }
                    }
                }
            }

            return CRAPatientXrayList;
        }
        private async Task<List<TbPatientPathologySurgical>> GetTbPatientPathologySurgicalData(string hn, DateTime? filterDate)
        {
            List<TbPatientPathologySurgical> CRAPatientPathologySurgicalList = new List<TbPatientPathologySurgical>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"SELECT * FROM tb_patient_pathology_surgical WHERE hn = @HN order by specimen_receive_datetime desc";
                if (filterDate.HasValue)
                {
                    sql = @"SELECT * FROM tb_patient_pathology_surgical WHERE hn = @HN and DATE(specimen_receive_datetime) = @filterDate order by specimen_receive_datetime desc";
                }
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@HN", hn);
                    if (filterDate.HasValue)
                    {
                        command.Parameters.AddWithValue("@filterDate", filterDate.Value.Date);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            TbPatientPathologySurgical patientPathologySurgicalData = new TbPatientPathologySurgical
                            {
                                // ID: Check for NULL if the C# property is int?
                                ID = reader.IsDBNull("ID") ? (int?)null : reader.GetInt32("ID"),

                                // String Fields: Use IsDBNull check for all
                                HN = reader.IsDBNull("HN") ? null : reader.GetString("HN"),
                                Facility_RMS_No = reader.IsDBNull("Facility_RMS_No") ? null : reader.GetString("Facility_RMS_No"),
                                Request_No = reader.IsDBNull("Request_No") ? null : reader.GetString("Request_No"),
                                Path_no = reader.IsDBNull("Path_no") ? null : reader.GetString("Path_no"),

                                // DateTime Fields: Check for NULL and cast to nullable DateTime?
                                Specimen_Receive_Datetime = reader.IsDBNull("Specimen_Receive_Datetime")? (DateTime?)null : reader.GetDateTime("Specimen_Receive_Datetime"),

                                Approve_Datetime = reader.IsDBNull("Approve_Datetime") ? (DateTime?)null: reader.GetDateTime("Approve_Datetime"),

                                Update_Date = reader.IsDBNull("Update_Date")? (DateTime?)null : reader.GetDateTime("Update_Date"),

                                // Rich_Text_Memo: CRITICAL check for NULL BEFORE calling GetString() and ConvertRtfToPlain()
                                Rich_Text_Memo = reader.IsDBNull("Rich_Text_Memo")
                     ? null
                     : RtfConverter.ConvertRtfToPlain(reader.GetString("Rich_Text_Memo")),

                                // Update_By: This was already correct, but rewritten for consistency
                                Update_By = reader.IsDBNull("Update_By") ? null : reader.GetString("Update_By")
                            };
                            CRAPatientPathologySurgicalList.Add(patientPathologySurgicalData);
                        }
                    }
                }
            }

            return CRAPatientPathologySurgicalList;
        }
        private async Task<List<TbPatientPathologyCyto>> GetTbPatientPathologyCytoData(string hn, DateTime? filterDate)
        {
            List<TbPatientPathologyCyto> CRATbPatientPathologyCytoList = new List<TbPatientPathologyCyto>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"SELECT * FROM tb_patient_pathology_cyto WHERE hn = @HN order by specimen_receive_datetime desc";
                if (filterDate.HasValue)
                {
                    sql = @"SELECT * FROM tb_patient_pathology_cyto WHERE hn = @HN and DATE(specimen_receive_datetime) = @filterDate order by specimen_receive_datetime desc";
                }
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@HN", hn);
                    if (filterDate.HasValue)
                    {
                        command.Parameters.AddWithValue("@filterDate", filterDate.Value.Date);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            TbPatientPathologyCyto patientPathologyCytoData = new TbPatientPathologyCyto
                            {
                                // ID: Check for NULL if ID is nullable (int or int?)
                                ID = reader.IsDBNull("ID") ? 0 : reader.GetInt32("ID"),
                                HN = reader.IsDBNull("HN") ? null : reader.GetString("HN"),
                                Facility_RMS_No = reader.IsDBNull("Facility_RMS_No") ? null : reader.GetString("Facility_RMS_No"),
                                Request_No = reader.IsDBNull("Request_No") ? null : reader.GetString("Request_No"),
                                Specimen_id = reader.IsDBNull("Specimen_id") ? null : reader.GetString("Specimen_id"),
                                // Rich_Text_Memo: CRITICAL check for NULL BEFORE calling GetString() and ConvertRtfToPlain()
                                Rich_Text_Memo = reader.IsDBNull("Rich_Text_Memo") ? null : RtfConverter.ConvertRtfToPlain(reader.GetString("Rich_Text_Memo")),

                                // DateTime Fields: Check for NULL and cast to nullable DateTime?
                                Specimen_Receive_Datetime = reader.IsDBNull("Specimen_Receive_Datetime") ? (DateTime?)null : reader.GetDateTime("Specimen_Receive_Datetime"),

                                Approve_Datetime = reader.IsDBNull("Approve_Datetime") ? (DateTime?)null : reader.GetDateTime("Approve_Datetime"),

                                Update_Date = reader.IsDBNull("Update_Date") ? (DateTime?)null : reader.GetDateTime("Update_Date"),

                                // Update_By: You already had a correct check for this one!
                                Update_By = reader.IsDBNull(reader.GetOrdinal("Update_By")) ? null : reader.GetString("Update_By")
                            };
                            CRATbPatientPathologyCytoList.Add(patientPathologyCytoData);
                        }
                    }
                }
            }

            return CRATbPatientPathologyCytoList;
        }
        private async Task<List<TbPatientPhysical>> GetTbPatientPhysicalData(string hn)
        {
            List<TbPatientPhysical> CRAPatientPhysicalList = new List<TbPatientPhysical>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"SELECT * FROM tb_patient_physical WHERE hn = @HN";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@HN", hn);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            TbPatientPhysical patientPhysicalData = new TbPatientPhysical
                            {
                                ID = reader.GetInt32("ID"),
                                HN = reader.GetString("HN"),
                                Facility_RMS_No = reader.GetString("Facility_RMS_No"),
                                Request_No = reader.GetString("Request_No"),
                                Update_Date = reader.GetDateTime("Update_Date"),
                                Update_By = reader.GetString("Update_By")
                            };
                            CRAPatientPhysicalList.Add(patientPhysicalData);
                        }
                    }
                }
            }

            return CRAPatientPhysicalList;
        }
        private async Task<string> GetTbPatientDiagnosisFirstRow(string hn)
        {
            string ICD_Text_EN = "";
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"SELECT * FROM tb_patient_diagnosis WHERE hn = @HN and diagnosis_record_type = '1'";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@HN", hn);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ICD_Text_EN = reader.IsDBNull("ICD_Text_EN") ? null : reader.GetString("ICD_Text_EN");
                        }
                    }
                }
            }

            return ICD_Text_EN;
        }
        private async Task<List<TbPatientDiagnosis>> GetTbPatientDiagnosisData(string hn)
        {
            List<TbPatientDiagnosis> CRATbPatientDiagnosisList = new List<TbPatientDiagnosis>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"SELECT * FROM tb_patient_diagnosis WHERE hn = @HN";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@HN", hn);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            TbPatientDiagnosis patientDiagnosisData = new TbPatientDiagnosis
                            {
                                ID = reader.GetInt32("ID"),
                                HN = reader.GetString("HN"),
                                Visit_Date = reader.GetDateTime("Visit_Date"),
                                VN = reader.GetString("VN"),
                                Prescription_No = reader.GetString("Prescription_No"),
                                Diagnosis_Record_Type = reader.GetString("Diagnosis_Record_Type"),
                                ICD_Code = reader.GetString("ICD_Code"),
                                ICD_Text_EN = reader.GetString("ICD_Text_EN"),
                                Update_Date = reader.GetDateTime("Update_Date"),
                                Update_By = reader.GetString("Update_By")
                            };
                            CRATbPatientDiagnosisList.Add(patientDiagnosisData);
                        }
                    }
                }
            }

            return CRATbPatientDiagnosisList;
        }
        private async Task<List<TbPatientHistory>> GetTbPatientHistoryData(string hn)
        {
            List<TbPatientHistory> CRAPatientHistoryList = new List<TbPatientHistory>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"SELECT * FROM tb_patient_history WHERE hn = @HN";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@HN", hn);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            TbPatientHistory patientHistoryData = new TbPatientHistory
                            {
                                ID = reader.GetInt32("ID"),
                                HN = reader.GetString("HN"),
                                IN_Date = reader.GetDateTime("IN_Date"),
                                Subjective_Data = reader.GetString("Subjective_Data"),
                                Update_Date = reader.GetDateTime("Update_Date"),
                                Update_By = reader.GetString("Update_By")
                            };
                            CRAPatientHistoryList.Add(patientHistoryData);
                        }
                    }
                }
            }

            return CRAPatientHistoryList;
        }
        private async Task<List<TbPatientHistory>> GetTbPatientHistoryDataFormat(string hn, DateTime? filterDate)
        {
            var CRAPatientHistoryList = new List<TbPatientHistory>();
            Int32 id = 0;
            string pid = "";
            DateTime? inDate;
            string subjectiveData = "";
            string Plan_Of_Management = "";
            string updateBy = "";
            string clinic = "";
            string clinic_name = "";
            string record_type = "";
            string vn = "";
            DateTime? visit_date;

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"SELECT * FROM tb_patient_history WHERE hn = @HN ORDER BY id";
                if (filterDate.HasValue)
                {
                    sql = @"SELECT * FROM tb_patient_history WHERE hn = @HN AND DATE(visit_date) = @filterDate ORDER BY id";
                }

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@HN", hn);
                    if (filterDate.HasValue)
                    {
                        command.Parameters.AddWithValue("@filterDate", filterDate.Value.Date);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // เช็คค่า Plan_Of_Management ป้องกัน NULL
                            bool hasPlan = !reader.IsDBNull(reader.GetOrdinal("Plan_Of_Management"));
                            string planValue = hasPlan ? reader.GetString("Plan_Of_Management") : "";

                            // Correct: Checking for NULL on 'vn'
                            bool hasVisit = !reader.IsDBNull(reader.GetOrdinal("vn"));
                            string visitValue = hasVisit ? reader.GetString("vn") : "";

                            // Safe reading for ID (assuming it is non-nullable INT, but added check for safety)
                            id = reader.IsDBNull("ID") ? 0 : Convert.ToInt32(reader["ID"]);

                            // Safe reading for string fields
                            pid = reader.IsDBNull("HN") ? null : reader["HN"].ToString();

                            // Safe reading for DateTime fields (Assuming IN_Date and visit_date are DateTime? in C#)
                            inDate = reader.IsDBNull("IN_Date") ? (DateTime?)null : reader.GetDateTime("IN_Date");

                            // Safe reading for Subjective_Data
                            subjectiveData = reader.IsDBNull("Subjective_Data") ? null : reader["Subjective_Data"].ToString();
                            subjectiveData = (subjectiveData ?? "") + "\n"; // If null, start with empty string + newline

                            // Safe reading for remaining string fields
                            updateBy = reader.IsDBNull("Update_By") ? null : reader["Update_By"].ToString();
                            // Plan_Of_Management is already handled by planValue
                            Plan_Of_Management = planValue;
                            clinic = reader.IsDBNull("clinic") ? null : reader["clinic"].ToString();
                            clinic_name = reader.IsDBNull("clinic_name") ? null : reader["clinic_name"].ToString();
                            record_type = reader.IsDBNull("record_type") ? null : reader["record_type"].ToString();

                            // Safe reading for visit_date
                            visit_date = reader.IsDBNull("visit_date") ? (DateTime?)null : reader.GetDateTime("visit_date");

                            // Post-processing Subjective Data
                            subjectiveData = (subjectiveData ?? string.Empty)
                                                     .Replace("Text^^System.String^", "")
                                                     .Replace("<br>", "\n");

                            // If you need to use the VN value:
                            // vn = visitValue;          

                            if (vn == visitValue)
                            {
                                if (hasPlan)
                                {
                                    Plan_Of_Management = planValue;
                                    Plan_Of_Management = Plan_Of_Management.Replace("Detail^^System.String^", "")
                                                                           .Replace("<br>", "\n")
                                                                           .Replace("^System.String^", "")
                                                                           .Replace("Tumor board conclusion <semicolon> ", "\nTumor board conclusion\n");
                                    subjectiveData = CRAPatientHistoryList[CRAPatientHistoryList.Count - 1].Subjective_Data;
                                    subjectiveData += "\nPlan Of Management\n" + Plan_Of_Management;
                                }

                                CRAPatientHistoryList.RemoveAt(CRAPatientHistoryList.Count - 1);
                            }
                            vn = reader.GetString("vn");

                            CRAPatientHistoryList.Add(new TbPatientHistory
                            {
                                ID = id,
                                HN = pid,
                                IN_Date = inDate,
                                Visit_date = visit_date,
                                VN = vn,
                                Subjective_Data = subjectiveData,
                                Update_Date = DateTime.Now,
                                Update_By = updateBy,
                                clinic = clinic,
                                clinic_name = clinic_name,
                                record_type = record_type
                            });
                        }
                    }
                }
            }

            return CRAPatientHistoryList;
        }
        [HttpGet]
        public async Task<IActionResult> GetTbPatientLabDataView(string hn, DateTime? filterDate)
        {
            if (examHN != "") hn = examHN;
            var labs = await GetTbPatientLabData(hn, filterDate);
            // Return a partial view or build HTML here
            return View("_LabResultTable", labs);
        }
        [HttpGet]
        public async Task<IActionResult> GetTbPatientImagingDataView(string hn, DateTime? filterDate)
        {
            try
            {
                var xray = await GetTbPatientXrayData(hn, filterDate);

                var model = new PatientImagingViewModel
                {
                    XrayList = xray,
                    FilterDate = filterDate,
                    HN = hn
                };

                //return View(model);
                return View("_ImagingTable", (model));
            }
            catch (Exception ex)
            {
                // ชั่วคราว: แสดง error message + StackTrace บนหน้าเว็บ
                return Content($"Error: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetTbPatientPathologyDataView(string hn, DateTime? filterDate)
        {
            if (examHN != "") hn = examHN;
            var surgical = await GetTbPatientPathologySurgicalData(hn, filterDate);
            var cyto = await GetTbPatientPathologyCytoData(hn, filterDate);
            // Return a partial view or build HTML here
            return View("_PathologyTable", Tuple.Create(surgical, cyto));
        }
        //[HttpGet]
        //public async Task<IActionResult> GetTbPatientHistoryDataView(string hn, string appointDate, DateTime? filterDate)
        //{
        //    DateTime? mdtDate = null;
        //    if (!string.IsNullOrEmpty(appointDate))
        //    {
        //        if (DateTime.TryParse(appointDate, out var parsed))
        //            mdtDate = parsed;
        //    }
        //    var history = await GetTbPatientHistoryDataFormat(hn, filterDate);
        //    var model = new PatientHistoryViewModel
        //    {
        //        HistoryList = history,
        //        AppointDatetime = mdtDate,
        //        FilterDate = filterDate
        //    };

        //    // ส่ง Tuple (history, mdtDate) ไป Partial View
        //    //return PartialView("_PatientHistoryTable", (history, mdtDate));
        //    return View(model);
        //}
        [HttpGet]
        public async Task<IActionResult> GetTbPatientHistoryDataView(string hn, string appointDate, DateTime? filterDate)
        {
            try
            {
                DateTime? mdtDate = null;
                if (!string.IsNullOrEmpty(appointDate) && DateTime.TryParse(appointDate, out var parsed))
                {
                    mdtDate = parsed;
                }

                var history = await GetTbPatientHistoryDataFormat(hn, filterDate);

                var model = new PatientHistoryViewModel
                {
                    HistoryList = history,
                    AppointDatetime = mdtDate,
                    FilterDate = filterDate,
                    HN = hn
                };

                //return View(model);
                return PartialView("_PatientHistoryTable", (model));
            }
            catch (Exception ex)
            {
                // ชั่วคราว: แสดง error message + StackTrace บนหน้าเว็บ
                return Content($"Error: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReport(string mdtDate)
        {
            // Query your data based on mdtDate or other filters
            var appointments = await _dbContext.CRA_MDT_Appointment
                .AsNoTracking()
                .Where(a => a.Appoint_Datetime.Date == DateTime.Parse(mdtDate).Date)
                .ToListAsync();

            // Generate a CSV file
            var csv = new StringBuilder();
            csv.AppendLine("HN,Name,Doctor,Date");
            foreach (var a in appointments)
                csv.AppendLine($"{a.HN},{a.First_Name} {a.Last_Name},{a.Doctor_Name},{a.Appoint_Datetime:yyyy-MM-dd}");

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Report_{mdtDate}.csv");
        }
        public async Task<IActionResult> MDTReport(string mdtDate)
        {
            // Fetch data for the report, filter by mdtDate if needed
            List<CRA_MDT_Appointment> appointments = await _dbContext.CRA_MDT_Appointment
                 .AsNoTracking()
                 .Where(a => a.Appoint_Datetime.Date == DateTime.Parse(mdtDate).Date)
                 .ToListAsync();
            List<CRA_MDT_Consult> consultMDT = await GetMDTDoctorConsult(DateTime.Parse(mdtDate).Date);

            // Return a tuple containing both lists to the view
            return View(Tuple.Create(appointments, consultMDT));
        }
        public async Task<IActionResult> MDTReportPdf(string mdtDate)
        {
            // Get your data
            var appointments = await _dbContext.CRA_MDT_Appointment
                .AsNoTracking()
                .Where(a => a.Appoint_Datetime.Date == DateTime.Parse(mdtDate).Date)
                .ToListAsync();

            var consults = await _dbContext.CRA_MDT_Consult
                .AsNoTracking()
                .Where(c => c.Appoint_Datetime.Date == DateTime.Parse(mdtDate).Date)
                .ToListAsync();

            // Create a tuple with both lists
            var model = Tuple.Create(appointments, consults);

            // Render Razor view to string
            var viewHtml = await RenderViewToStringAsync("MDTReport", model);

            //// Insert a table line (horizontal rule) before the first table
            //var tableLineHtml = "<hr style=\"border:1px solid #000;margin-bottom:10px;\" />";
            //int tableIndex = viewHtml.IndexOf("<table", StringComparison.OrdinalIgnoreCase);
            //if (tableIndex >= 0)
            //{
            //    viewHtml = viewHtml.Insert(tableIndex, tableLineHtml);
            //}

            // Convert HTML to PDF
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Landscape,
                    Margins = new MarginSettings { Top = 10, Bottom = 10 }
                },
                Objects = {
                    new ObjectSettings() {
                        HtmlContent = viewHtml
                    }
                }
            };

            var pdf = _converter.Convert(doc);
            return File(pdf, "application/pdf", "MDTReport.pdf");
        }
        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor);
            using var sw = new StringWriter();
            var viewResult = _viewEngine.FindView(actionContext, viewName, false);

            if (viewResult.View == null)
                throw new ArgumentNullException($"{viewName} does not match any available view");

            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                new TempDataDictionary(HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            var html = sw.ToString();

            // เพิ่ม CSS เพื่อแสดงเส้นของตาราง (border)
            var tableBorderStyle = @"<style>
                table, th, td {
                    border: 1px solid #000 !important;
                    border-collapse: collapse !important;
                }
                th, td {
                    padding: 4px 8px;
                }
            </style>";
            int headIndex = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
            if (headIndex >= 0)
            {
                html = html.Insert(headIndex, tableBorderStyle);
            }
            else
            {
                // ถ้าไม่มี <head> ให้แทรก style ไว้ต้น HTML
                html = tableBorderStyle + html;
            }

            return html;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveConsult(string consult, DateTime mdtDate)
        {
            if (string.IsNullOrWhiteSpace(consult))
            {
                return Json(new { success = false, message = "Consult value is required." });
            }

            try
            {
                var record = await _dbContext.CRA_MDT_Consult
                    .FirstOrDefaultAsync(a => a.Appoint_Datetime.Date == mdtDate.Date);

                if (record == null)
                {
                    var consultEntity = new CRA_MDT_Consult
                    {
                        Doctor_Consult = consult,
                        Appoint_Datetime = mdtDate
                    };
                    _dbContext.CRA_MDT_Consult.Add(consultEntity);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    record.Doctor_Consult = consult;
                    _dbContext.CRA_MDT_Consult.Update(record);
                    await _dbContext.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveSelectedXrays([FromBody] List<string> selectedRequestNo)
        {
            if (selectedRequestNo == null || !selectedRequestNo.Any())
                return Json(new { success = false, message = "No items selected." });

            foreach (var requestNo in selectedRequestNo)
            {
                CRA_Temp_Patient_xRay tempXray = null; 
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string sql = @"SELECT * FROM tb_patient_xray WHERE request_no = @RequestNo";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@RequestNo", requestNo);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tempXray = new CRA_Temp_Patient_xRay
                                {
                                    Xray_Date = reader.GetDateTime("Confirm_Result_Datetime"),
                                    HN = reader.GetString("HN"),
                                    Request_No = reader.GetString("Request_No"),
                                    Confirm_Result_Datetime = reader.GetDateTime("Confirm_Result_Datetime"),
                                    Rich_Text_Memo = RtfConverter.ConvertRtfToPlainTextSimple(reader.GetString("Rich_Text_Memo")),
                                    Confirm_Date = DateTime.Now,
                                    Confirm_by = User.Identity?.Name ?? "System"
                                };
                            }
                        }
                    }
                }

                if (tempXray != null) // Ensure tempXray is not null before adding it to the DbContext
                {
                    CRA_DIP_FFMessage craFF = new CRA_DIP_FFMessage();
                    //string fileContent = craFF.CRA_FF_RADDiagnosticReport(tempXray.Request_No, "Imaging", tempXray.Confirm_Date?.ToString("yyyy-MM-dd"), tempXray.Rich_Text_Memo);
                    _dbContext.CRA_Temp_Patient_xRay.Add(tempXray);
                }
            }
            await _dbContext.SaveChangesAsync();

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> SaveSingleXray(string requestNo, string hn)
        {
            // 1. ตรวจสอบค่าที่ส่งมา
            if (string.IsNullOrEmpty(requestNo) || string.IsNullOrEmpty(hn))
            {
                return Json(new { success = false, message = "Request number or HN is missing." });
            }

            try
            {
                // 2. โค้ดส่วนนี้คือ LOGIC ในการบันทึกข้อมูล X-ray เข้าสู่ฐานข้อมูล
                //    คุณจะต้องเรียกใช้ Service หรือ Repository เพื่อบันทึกข้อมูล

                //bool isSaved = SaveXrayRecordToDatabase(requestNo, hn); // ***แทนที่ด้วย Logic จริงของคุณ***

                CRA_Temp_Patient_xRay tempXray = null;
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string sql = @"SELECT * FROM tb_patient_xray WHERE request_no = @RequestNo";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@RequestNo", requestNo);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tempXray = new CRA_Temp_Patient_xRay
                                {
                                    Xray_Date = reader.GetDateTime("date_of_xray"),
                                    HN = reader.GetString("HN"),
                                    Request_No = reader.GetString("Request_No"),
                                    Confirm_Result_Datetime = reader.GetDateTime("Confirm_Result_Datetime"),
                                    Rich_Text_Memo = RtfConverter.ConvertRtfToPlainTextSimple(reader.GetString("Rich_Text_Memo")),
                                    Confirm_Date = DateTime.Now,
                                    Confirm_by = User.Identity?.Name ?? "System",
                                    xray_name_english = reader.GetString("xray_name_english"),
                                    xray_code = reader.GetString("xray_code")
                                };
                            }
                        }
                    }
                }

                if (tempXray != null) // Ensure tempXray is not null before adding it to the DbContext
                {
                    CRA_DIP_FFMessage craFF = new CRA_DIP_FFMessage();
                    //string fileContent = craFF.CRA_FF_RADDiagnosticReport(tempXray.Request_No, "Imaging", tempXray.Confirm_Date?.ToString("yyyy-MM-dd"), tempXray.Rich_Text_Memo);
                    _dbContext.CRA_Temp_Patient_xRay.Add(tempXray);
                }
                await _dbContext.SaveChangesAsync();
                return Json(new { success = true, message = $"X-ray record {requestNo} saved for patient {hn}." });

                //if (true)
                //{
                //    // 3. ส่งผลลัพธ์ว่าสำเร็จ
                //    return Json(new { success = true, message = $"X-ray record {requestNo} saved for patient {hn}." });
                //}
                //else
                //{
                //    // 4. ส่งผลลัพธ์ว่าล้มเหลว (หาก Logic การบันทึกคืนค่าเป็น false)
                //    return Json(new { success = false, message = $"Failed to save X-ray record {requestNo} to the database." });
                //}
            }
            catch (Exception ex)
            {
                // 5. จัดการ Error ที่เกิดขึ้น
                // Log the exception (แนะนำให้ log error ที่นี่)
                return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveHistory(string HN, string SubjectiveData, DateTime Appoint_Datetime)
        {
            string mdtName = "Breast";
            if (string.IsNullOrWhiteSpace(HN) || string.IsNullOrWhiteSpace(SubjectiveData))
            {
                return Json(new { success = false, message = "HN or Subjective Data is missing." });
            }

            try
            {
                var existing = await _dbContext.CRA_Temp_Patient_History.FindAsync(Appoint_Datetime.Date, HN, mdtName);
                if (existing != null)
                {
                    existing.patient_history = existing.patient_history + SubjectiveData;
                    existing.Update_Date = DateTime.Now;
                    existing.Update_by = User.Identity?.Name ?? "system";
                    _dbContext.CRA_Temp_Patient_History.Update(existing);
                }
                else
                {
                    var tempHistory = new CRA_Temp_Patient_History
                    {
                        HN = HN,
                        patient_history = SubjectiveData,
                        MDT_Date = Appoint_Datetime,
                        Update_Date = DateTime.Now,
                        MDT_Name = "Breast",
                        Update_by = User.Identity?.Name ?? "system"
                    };
                    _dbContext.CRA_Temp_Patient_History.Add(tempHistory);
                }

                await _dbContext.SaveChangesAsync();
                return Json(new { success = true, message = "Saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSinglePathology(string requestNo, string hn, string histoType, string pathNo)
        {
            // 1. ตรวจสอบค่าที่ส่งมา
            if (string.IsNullOrEmpty(requestNo) || string.IsNullOrEmpty(hn))
            {
                return Json(new { success = false, message = "Request number or HN is missing." });
            }

            try
            {
                // 2. โค้ดส่วนนี้คือ LOGIC ในการบันทึกข้อมูล X-ray เข้าสู่ฐานข้อมูล
                //    คุณจะต้องเรียกใช้ Service หรือ Repository เพื่อบันทึกข้อมูล

                //bool isSaved = SaveXrayRecordToDatabase(requestNo, hn); // ***แทนที่ด้วย Logic จริงของคุณ***

                CRA_Temp_Patient_Pathology tempPathology = null;
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string sql = "";
                    if (histoType == "Cyto")
                        sql = @"SELECT *, specimen_id as path_no FROM tb_patient_pathology_cyto WHERE request_no = @RequestNo and hn =@hn and specimen_id = @path_no";

                    if(histoType == "Surgical")
                        sql = @"SELECT * FROM tb_patient_pathology_surgical WHERE request_no = @RequestNo and hn =@hn and path_no = @path_no";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@RequestNo", requestNo);
                        command.Parameters.AddWithValue("@hn", hn);
                        command.Parameters.AddWithValue("@path_no", pathNo);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tempPathology = new CRA_Temp_Patient_Pathology
                                {
                                    HN = reader.GetString("HN"),
                                    facility_rms_no = reader.GetString("facility_rms_no"),
                                    request_no = reader.GetString("request_no"),
                                    specimen_receive_datetime = reader.GetDateTime("specimen_receive_datetime"),
                                    approve_datetime = reader.GetDateTime("approve_datetime"),
                                    rich_text_memo = RtfConverter.ConvertRtfToPlainTextSimple(reader.GetString("Rich_Text_Memo")),
                                    update_date = DateTime.Now,
                                    update_by = User.Identity?.Name ?? "System",
                                    path_no = reader.GetString("path_no"),
                                    lab_code = reader.GetString("lab_code"),
                                    lab_name_english = reader.GetString("lab_name_english"),
                                    histo_type = histoType
                                };
                            }
                        }
                    }
                }

                if (tempPathology != null) // Ensure tempXray is not null before adding it to the DbContext
                {
                    //CRA_DIP_FFMessage craFF = new CRA_DIP_FFMessage();
                    //string fileContent = craFF.CRA_FF_RADDiagnosticReport(tempXray.Request_No, "Imaging", tempXray.Confirm_Date?.ToString("yyyy-MM-dd"), tempXray.Rich_Text_Memo);
                    _dbContext.CRA_Temp_Patient_Pathology.Add(tempPathology);
                }
                await _dbContext.SaveChangesAsync();
                return Json(new { success = true, message = $"Pathology record {requestNo} saved for patient {hn}." });

                //if (true)
                //{
                //    // 3. ส่งผลลัพธ์ว่าสำเร็จ
                //    return Json(new { success = true, message = $"X-ray record {requestNo} saved for patient {hn}." });
                //}
                //else
                //{
                //    // 4. ส่งผลลัพธ์ว่าล้มเหลว (หาก Logic การบันทึกคืนค่าเป็น false)
                //    return Json(new { success = false, message = $"Failed to save X-ray record {requestNo} to the database." });
                //}
            }
            catch (Exception ex)
            {
                // 5. จัดการ Error ที่เกิดขึ้น
                // Log the exception (แนะนำให้ log error ที่นี่)
                return Json(new { success = false, message = "An unexpected error occurred: " + ex.Message });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSelectedLabs([FromBody] List<CRA_Temp_Patient_Lab> labDataList)
        {
            if (labDataList == null || !labDataList.Any())
            {
                return Json(new { success = false, message = "No lab items received." });
            }
            try
            {
                CRA_Temp_Patient_Lab tempLab = null;
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string sql = @"SELECT * FROM tb_patient_lab WHERE request_no = @RequestNo and hn =@hn and lab_code = @lab_code";

                    foreach (var labItem in labDataList)
                    {
                        using (var command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@RequestNo", labItem.request_no);
                            command.Parameters.AddWithValue("@hn", labItem.HN);
                            command.Parameters.AddWithValue("@lab_code", labItem.lab_code);

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    tempLab = new CRA_Temp_Patient_Lab
                                    {
                                        HN = reader.GetString("HN"),
                                        request_no = reader.GetString("request_no"),
                                        lab_code = reader.GetString("lab_code"),
                                        lab_name_english = reader.GetString("lab_name_english"),
                                        result_value = reader.GetString("result_value"),
                                        result_datetime = reader.GetDateTime("result_datetime"),
                                        previous_result_datetime = reader.GetDateTime("previous_result_datetime"),
                                        previous_result_value = reader.GetString("previous_result_value"),
                                        lab_unit = reader.GetString("lab_unit"),

                                        update_date = DateTime.Now,
                                        update_by = User.Identity.Name ?? "System",

                                        request_lab_code = reader.GetString("request_lab_code"),
                                        request_lab_name_english = reader.GetString("request_lab_name_english"),
                                    };

                                    _dbContext.CRA_Temp_Patient_Lab.Add(tempLab);
                                }
                            }
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();

                return Json(new { success = true, message = $"Successfully processed and saved {labDataList.Count} lab items." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred during bulk save: " + ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> CheckTempPatientHistory(string hn, DateTime mdtDate, string mdtName)
        {
            var tempHistory = await _dbContext.CRA_Temp_Patient_History
                .FirstOrDefaultAsync(x => x.HN == hn && x.MDT_Date == mdtDate.Date && x.MDT_Name == mdtName);

            if (tempHistory == null)
                return Json(new { exists = false });

            // Return the data if exists
            return Json(new
            {
                exists = true,
                data = new
                {
                    hn = tempHistory.HN,
                    mdtDate = tempHistory.MDT_Date?.ToString("yyyy-MM-dd"),
                    mdtName = tempHistory.MDT_Name,
                    patient_history = tempHistory.patient_history,
                    update_by = tempHistory.Update_by,
                    update_date = tempHistory.Update_Date?.ToString("yyyy-MM-dd HH:mm")
                }
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetTempPatientHistory(string hn, DateTime appointDate, string mdtName)
        {
            if (string.IsNullOrEmpty(hn) || appointDate == default(DateTime))
            {
                return BadRequest("HN and Appointment Date are required.");
            }
            try {
                var tempHistory = await _dbContext.CRA_Temp_Patient_History
                    .FirstOrDefaultAsync(x => x.HN == hn && x.MDT_Date == appointDate.Date && x.MDT_Name == mdtName);
                if (tempHistory == null)
                    return NotFound($"MDT Appointment not found for HN: {hn} on {appointDate.ToShortDateString()}.");

                return View("_PatientHistoryTemp", tempHistory);
            }
            catch (Exception ex) {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateTempPatientHistory(string hn, DateTime mdtDate, string mdtName, string patientHistory)
        {
            var tempHistory = await _dbContext.CRA_Temp_Patient_History
                .FirstOrDefaultAsync(x => x.HN == hn && x.MDT_Date == mdtDate.Date && x.MDT_Name == mdtName);

            if (tempHistory == null)
                return Json(new { success = false, message = "Record not found for update." });

            tempHistory.patient_history = patientHistory;
            tempHistory.Update_Date = DateTime.Now;
            tempHistory.Update_by = User.Identity?.Name ?? "system";
            // Optionally update Update_by here
            _dbContext.CRA_Temp_Patient_History.Update(tempHistory);
            await _dbContext.SaveChangesAsync();

            return Json(new { success = true, message = "Updated successfully." });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTempPatientHistory(string hn, DateTime mdtDate, string mdtName)
        {
            var tempHistory = await _dbContext.CRA_Temp_Patient_History
                .FirstOrDefaultAsync(x => x.HN == hn && x.MDT_Date == mdtDate.Date && x.MDT_Name == mdtName);

            if (tempHistory == null)
                return Json(new { success = false, message = "Record not found." });

            _dbContext.CRA_Temp_Patient_History.Remove(tempHistory);
            await _dbContext.SaveChangesAsync();

            return Json(new { success = true, message = "Deleted successfully." });
        }
    }
}
