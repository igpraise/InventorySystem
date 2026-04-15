// File: AdminController.cs
// Student: Chinonso Praise Ignatius
// Course: SECU2000 - Application Security
// Description: handles all admin only operations
//              only admin users can access these endpoints (Week 4)
//              all admin actions are logged (Week 12)

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using InventoryAPI.Models;

namespace InventoryAPI.Controllers
{
    // this controller handles all admin endpoints
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        // database helper to connect to sql server
        private readonly DatabaseHelper _db;

        // constructor - gets the database helper from dependency injection
        public AdminController(DatabaseHelper db)
        {
            // store the database helper
            _db = db;
        }

        // gets all users in the system, only admins can do this
        [HttpGet("users")]
        public IActionResult GetAllUsers([FromQuery] string role)
        {
            // check if the user is an admin (Week 4 - authorization)
            if (role != "admin")
            {
                return Unauthorized(new { message = "Only admins can view all users" });
            }

            // list to store all users
            List<User> users = new List<User>();

            // connect to the database
            using (SqlConnection connection = _db.CreateConnection())
            {
                // open the connection
                connection.Open();

                // get all users using parameterized query (Week 3)
                string query = "SELECT user_id, full_name, email, role, is_active, created_at FROM Users";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // read all the results
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // build each user from the database result
                            users.Add(new User
                            {
                                UserId = (int)reader["user_id"],
                                FullName = reader["full_name"].ToString() ?? "",
                                Email = reader["email"].ToString() ?? "",
                                Role = reader["role"].ToString() ?? "user",
                                IsActive = (bool)reader["is_active"],
                                CreatedAt = (DateTime)reader["created_at"]
                            });
                        }
                    }
                }
            }

            // return the list of users
            return Ok(users);
        }

        // disables a user account, only admins can do this
        [HttpPut("disable/{userId}")]
        public IActionResult DisableUser(int userId, [FromQuery] string role, [FromQuery] int adminId)
        {
            // check if the user is an admin (Week 4 - authorization)
            if (role != "admin")
            {
                return Unauthorized(new { message = "Only admins can disable users" });
            }

            // connect to the database and disable the user
            using (SqlConnection connection = _db.CreateConnection())
            {
                // open the connection
                connection.Open();

                // use parameterized query to prevent sql injection (Week 3)
                string query = "UPDATE Users SET is_active = 0 WHERE user_id = @UserId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // add the user id parameter safely
                    command.Parameters.AddWithValue("@UserId", userId);

                    // run the update command
                    command.ExecuteNonQuery();
                }
            }

            // log the admin action (Week 12)
            LogAction(adminId, "Admin disabled user: " + userId, GetIpAddress(), "success");

            // return success message
            return Ok(new { message = "User disabled successfully" });
        }

        // approves a receipt upload, only admins can do this
        [HttpPut("approve/{receiptId}")]
        public IActionResult ApproveReceipt(int receiptId, [FromQuery] string role, [FromQuery] int adminId)
        {
            // check if the user is an admin (Week 4 - authorization)
            if (role != "admin")
            {
                return Unauthorized(new { message = "Only admins can approve receipts" });
            }

            // connect to the database and approve the receipt
            using (SqlConnection connection = _db.CreateConnection())
            {
                // open the connection
                connection.Open();

                // use parameterized query to prevent sql injection (Week 3)
                string query = "UPDATE Receipts SET status = 'approved' WHERE receipt_id = @ReceiptId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // add the receipt id parameter safely
                    command.Parameters.AddWithValue("@ReceiptId", receiptId);

                    // run the update command
                    command.ExecuteNonQuery();
                }
            }

            // log the admin action (Week 12)
            LogAction(adminId, "Admin approved receipt: " + receiptId, GetIpAddress(), "success");

            // return success message
            return Ok(new { message = "Receipt approved successfully" });
        }

        // gets all logs in the system, only admins can do this
        [HttpGet("logs")]
        public IActionResult GetAllLogs([FromQuery] string role)
        {
            // check if the user is an admin (Week 4 - authorization)
            if (role != "admin")
            {
                return Unauthorized(new { message = "Only admins can view logs" });
            }

            // list to store all logs
            List<Log> logs = new List<Log>();

            // connect to the database
            using (SqlConnection connection = _db.CreateConnection())
            {
                // open the connection
                connection.Open();

                // get all logs using parameterized query (Week 3)
                string query = "SELECT log_id, user_id, action, ip_address, status, logged_at FROM Logs ORDER BY logged_at DESC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // read all the results
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // build each log from the database result
                            logs.Add(new Log
                            {
                                LogId = (int)reader["log_id"],
                                UserId = reader["user_id"] == DBNull.Value ? null : (int)reader["user_id"],
                                Action = reader["action"].ToString() ?? "",
                                IpAddress = reader["ip_address"].ToString() ?? "",
                                Status = reader["status"].ToString() ?? "",
                                LoggedAt = (DateTime)reader["logged_at"]
                            });
                        }
                    }
                }
            }

            // return the list of logs
            return Ok(logs);
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