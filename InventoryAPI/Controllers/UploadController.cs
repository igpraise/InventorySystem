// File: UploadController.cs
// Student: Chinonso Praise Ignatius
// Course: SECU2000 - Application Security
// Description: handles file uploads for receipts
//              checks file type and size before saving (Week 6)
//              logs every upload attempt (Week 12)

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using InventoryAPI.Models;

namespace InventoryAPI.Controllers
{
    // this controller handles all file upload endpoints
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        // database helper to connect to sql server
        private readonly DatabaseHelper _db;

        // only allow these file types to be uploaded (Week 6)
        private readonly string[] _allowedTypes = { ".pdf", ".jpg", ".jpeg", ".png" };

        // max file size is 2mb in bytes (Week 6)
        private readonly long _maxFileSize = 2 * 1024 * 1024;

        // constructor - gets the database helper from dependency injection
        public UploadController(DatabaseHelper db)
        {
            // store the database helper
            _db = db;
        }

        // handles receipt file uploads from users
        [HttpPost("receipt")]
        public IActionResult UploadReceipt(IFormFile file, [FromQuery] int userId, [FromQuery] int itemId)
        {
            // check if a file was actually sent
            if (file == null || file.Length == 0)
            {
                // log the failed upload (Week 12)
                LogAction(userId, "Upload failed - no file sent", GetIpAddress(), "failure");
                return BadRequest(new { message = "Please select a file to upload" });
            }

            // check the file size is not too big (Week 6)
            if (file.Length > _maxFileSize)
            {
                // log the failed upload (Week 12)
                LogAction(userId, "Upload failed - file too large", GetIpAddress(), "failure");
                return BadRequest(new { message = "File size must be under 2MB" });
            }

            // get the file extension and check it is allowed (Week 6)
            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedTypes.Contains(fileExtension))
            {
                // log the failed upload (Week 12)
                LogAction(userId, "Upload failed - invalid file type: " + fileExtension, GetIpAddress(), "failure");
                return BadRequest(new { message = "Only PDF, JPG and PNG files are allowed" });
            }

            // create a safe unique file name to prevent path traversal (Week 6)
            string safeFileName = Guid.NewGuid().ToString() + fileExtension;

            // create the uploads folder if it does not exist
            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // save the file to the uploads folder
            string filePath = Path.Combine(uploadFolder, safeFileName);
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                // copy the file to the server
                file.CopyTo(stream);
            }

            // get file size in kb
            int fileSizeKb = (int)(file.Length / 1024);

            // save the file info to the database
            using (SqlConnection connection = _db.CreateConnection())
            {
                // open the connection
                connection.Open();

                // use parameterized query to prevent sql injection (Week 3)
                string query = "INSERT INTO Receipts (file_name, file_path, file_type, file_size_kb, uploaded_by, item_id) VALUES (@FileName, @FilePath, @FileType, @FileSizeKb, @UploadedBy, @ItemId)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // add all parameters safely
                    command.Parameters.AddWithValue("@FileName", file.FileName);
                    command.Parameters.AddWithValue("@FilePath", filePath);
                    command.Parameters.AddWithValue("@FileType", fileExtension);
                    command.Parameters.AddWithValue("@FileSizeKb", fileSizeKb);
                    command.Parameters.AddWithValue("@UploadedBy", userId);
                    command.Parameters.AddWithValue("@ItemId", itemId);

                    // run the insert command
                    command.ExecuteNonQuery();
                }
            }

            // log the successful upload (Week 12)
            LogAction(userId, "File uploaded successfully: " + file.FileName, GetIpAddress(), "success");

            // return success message
            return Ok(new { message = "File uploaded successfully", fileName = safeFileName });
        }

        // saves a log entry to the database (Week 12)
        private void LogAction(int? userId, string action, string ipAddress, string status)
        {
            // connect to the database and insert the log
            using (SqlConnection connection = _db.CreateConnection())
            {
                // open the connection
                connection.Open();

                // insert the log using parameterized query
                string query = "INSERT INTO Logs (user_id, action, ip_address, status) VALUES (@UserId, @Action, @IpAddress, @Status)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // add all parameters safely
                    command.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Action", action);
                    command.Parameters.AddWithValue("@IpAddress", ipAddress);
                    command.Parameters.AddWithValue("@Status", status);

                    // run the insert
                    command.ExecuteNonQuery();
                }
            }
        }

        // gets the ip address of the person making the request
        private string GetIpAddress()
        {
            // return the ip address from the request
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}