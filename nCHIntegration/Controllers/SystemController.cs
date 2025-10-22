using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

[Route("[controller]")]
public class SystemController : Controller
{
    private readonly IConfiguration _configuration;

    public SystemController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("CheckDbConnection")]
    public async Task<IActionResult> CheckDbConnection([FromBody] string connectionName)
    {
        string connectionString = _configuration.GetConnectionString(connectionName);
        string dbType = connectionName.Equals("Default", StringComparison.OrdinalIgnoreCase) ? "SQL Server" : "MySQL";

        if (string.IsNullOrEmpty(connectionString))
        {
            return Json(new { success = false, message = $"Connection string '{connectionName}' not found in configuration." });
        }

        try
        {
            bool isSuccess = false;

            if (connectionName.Equals("Default", StringComparison.OrdinalIgnoreCase))
            {
                // Logic สำหรับ SQL Server (Default: Windows Auth)
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    isSuccess = true;
                }
            }
            else if (connectionName.Equals("CRAConnectionString", StringComparison.OrdinalIgnoreCase))
            {
                // Logic สำหรับ MySQL (CRAConnectionString: User/Pwd Auth)
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    isSuccess = true;
                }
            }

            if (isSuccess)
            {
                return Json(new { success = true, message = $"{dbType} connection '{connectionName}' is successful. Server: {connectionString.Split(';')[0]}" });
            }
            else
            {
                return Json(new { success = false, message = $"Unknown connection type or connection string '{connectionName}' does not match expected format." });
            }
        }
        catch (Exception ex)
        {
            // ตรวจสอบชนิดของ Exception เพื่อรายงานให้ชัดเจน
            string specificError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return Json(new { success = false, message = $"{dbType} connection '{connectionName}' failed. Error: {specificError}" });
        }
    }
}