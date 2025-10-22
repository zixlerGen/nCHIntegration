using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using nCHIntegration.Data;
using nCHIntegration.DIP;
using nCHIntegration.Models;
using nCHIntegration.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace nCHIntegration.Controllers
{
    public class PatientCRAController : Controller
    {
        private readonly string _connectionString;
        private readonly AppDBContext _dbContext;
        public PatientCRAController(IConfiguration configuration, AppDBContext dbContext)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _connectionString = configuration.GetConnectionString("CRAConnectionString")
                                ?? throw new InvalidOperationException("CRAConnectionString is not configured.");
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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
            List<CRA_MDT_Appointment> appointmentsMDT  = await GetDataFromMySQL(mdtDate);
            //List<CRA_MDT_Appointment> appointmentsMDT = await GetMDTAppointmentsFromSqlServer(mdtDate);
            List<CRA_Patient_Right> patientRights = await GetPatientRights();
            List<CRA_MDT_Group> mdtGroup = await GetCRAMDTGroup();
            //return View(appointmentsMDT);
            return View(Tuple.Create(appointmentsMDT, patientRights, mdtGroup));
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
                                HN = reader.GetString("HN"),
                                Appoint_Datetime = reader.GetDateTime("Appoint_Datetime"),
                                Appoint_No = reader.GetString("Appoint_No"),
                                Clinic_Code = reader.GetString("Clinic_Code"),
                                Clinic_Name = reader.GetString("Clinic_Name"),
                                Initial_Name_Code = reader.GetString("Initial_Name_Code"),
                                Initial_Name = reader.GetString("Initial_Name"),
                                First_Name = reader.GetString("First_Name"),
                                Last_Name = reader.GetString("Last_Name"),
                                Gender = reader.GetString("Gender"),
                                Gender_Name = reader.GetString("Gender_Name"),
                                Birth_Datetime = reader.GetDateTime("Birth_Datetime"),
                                Right_Code = reader.GetString("Right_Code"),
                                Right_Name = reader.GetString("Right_Name"),
                                Address1 = reader.GetString("Address1"),
                                Province = reader.GetString("Province"),
                                Province_Name = reader.GetString("Province_Name"),
                                Amphoe = reader.GetString("Amphoe"),
                                Amphoe_Name = reader.GetString("Amphoe_Name"),
                                Tambon = reader.GetString("Tambon"),
                                Tambon_Name = reader.GetString("Tambon_Name"),
                                Postal_Code = reader.GetString("Postal_Code"),
                                Doctor = reader.GetString("Doctor"),
                                Doctor_Name = reader.GetString("Doctor_Name"),
                                CTB_No = reader.GetString("CTB_No"),
                                Remarks_Memo = reader.GetString("Remarks_Memo"),
                                Update_Date = reader.GetDateTime("Update_Date"),
                                Update_By = reader.GetString("Update_By")
                            };
                            //appointments.Add(appointment);
                            await InsertMDTPatientList(appointment);
                        }
                    }
                }
            }
            List<CRA_MDT_Appointment> appointmentsMDTList = await GetMDTAppointmentsFromSqlServer(mdtDate);
            return appointmentsMDTList;
        }
        private CRA_MDT_Appointment MapAppointmentToMDTPatientList(CRA_PatientAppointment appointment)
        {
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
                MDT_Visit = 5,
                MDT_Consult = null,
                MDT_Review_Image = null,
                MDT_Appointment = null,
                Update_Date = null,
                update_by = null,
                confirm_status = false
            };
        }

        public async Task<IActionResult> InsertMDTPatientList(CRA_PatientAppointment appointment)
        {
            try
            {
                var record = _dbContext.CRA_MDT_Appointment
                    .FirstOrDefault(a => a.HN == appointment.HN && a.Appoint_Datetime.Date == appointment.Appoint_Datetime);

                if (record == null)
                {
                    CRA_MDT_Appointment mdtPatientList = MapAppointmentToMDTPatientList(appointment);
                    _dbContext.CRA_MDT_Appointment.Add(mdtPatientList);
                    await _dbContext.SaveChangesAsync();
                }

                return Ok("Data inserted into CRA_MDTPatientList successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
        // Add this method to PatientCRAController

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
        public async Task<List<CRA_Patient_Right>> GetPatientRights()
        {
            var patientrights = await _dbContext.CRA_Patient_Right
                .ToListAsync();

            return patientrights;
        }
        [HttpGet]
        public async Task<List<CRA_MDT_Group>> GetCRAMDTGroup()
        {
            var mdtGroup = await _dbContext.CRA_MDT_Group
                .ToListAsync();

            return mdtGroup;
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
                    record.Doctor_Name = form["Doctor_Name"].ToString();
                    record.Right_Name = form["Patient_Rights"].ToString();
                    record.MDT_Group = form["MDT_Group"].ToString();
                    record.MDT_Consult = form["consult"].ToString();
                    record.MDT_Review_Image = form["reviewimage"].ToString();
                    record.MDT_Appointment = form["appointment"].ToString();
                    record.update_by = "System";
                    record.Update_Date = DateTime.Now;
                    if (isSendToNch)
                    {
                        record.confirm_status = true;
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
                    existingAppointment.update_by = "System";
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
                    string fileName = $"{item.HN}_01_patient_flatfile_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    HL7Message resultFF = WriteDataToFlatFile(FFMessageFormat, item.HN, "PatientDemographic", fileName);

                    // Update the status
                    item.confirm_status = true;
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

                return Json(new { success = true, message = "Send to nCH success.", mdtDate = mdtDateToRefresh?.ToString("yyyy-MM-dd") });
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

    }
}
