// File: AuthController.cs
// Student: Chinonso Praise Ignatius
// Course: SECU2000 - Application Security
// Description: handles login and registration for the app
//              passwords are hashed using bcrypt (Week 4)
//              all login attempts are logged (Week 12)

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using InventoryAPI.Models;

namespace InventoryAPI.Controllers
{
    // this controller handles all authentication endpoints
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // database helper to connect to sql server
        private readonly DatabaseHelper _db;

        // constructor - gets the database helper from dependency injection
        public AuthController(DatabaseHelper db)
        {
            // store the database helper
            _db = db;
        }

        // handles login requests from the login form
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // check if email or password is empty
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                // log the failed attempt
                LogAction(null, "Login attempt with empty fields", GetIpAddress(), "failure");
                return BadRequest(new { message = "Email and password are required" });
            }

            // get the user from the database using parameterized query (Week 3)
            User? foundUser = null;
            using (SqlConnection connection = _db.CreateConnection())
            {
                // open the connection
                connection.Open();

                // use parameterized query to prevent sql injection (Week 3)
                string query = "SELECT user_id, full_name, email, password_hash, role, is_active FROM Users WHERE email = @Email";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // add the email as a parameter safely
                    command.Parameters.AddWithValue("@Email", request.Email);

                    // read the result
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // build the user object from the database result
                            foundUser = new User
                            {
                                UserId = (int)reader["user_id"],
                                FullName = reader["full_name"].ToString() ?? "",
                                Email = reader["email"].ToString() ?? "",
                                PasswordHash = reader["password_hash"].ToString() ?? "",
                                Role = reader["role"].ToString() ?? "user",
                                IsActive = (bool)reader["is_active"]
                            };
                        }
                    }
                }
            }

            // check if user was not found
            if (foundUser == null)
            {
                // log the failed login attempt (Week 12)
                LogAction(null, "Login failed - user not found: " + request.Email, GetIpAddress(), "failure");
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // check if account is disabled
            if (!foundUser.IsActive)
            {
                // log the failed login attempt (Week 12)
                LogAction(foundUser.UserId, "Login failed - account disabled", GetIpAddress(), "failure");
                return Unauthorized(new { message = "Account is disabled" });
            }

            // verify the password using bcrypt (Week 4)
            bool passwordMatch = BCrypt.Net.BCrypt.Verify(request.Password, foundUser.PasswordHash);
            if (!passwordMatch)
            {
                // log the failed login attempt (Week 12)
                LogAction(foundUser.UserId, "Login failed - wrong password", GetIpAddress(), "failure");
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // log the successful login (Week 12)
            LogAction(foundUser.UserId, "Login successful", GetIpAddress(), "success");

            // return the user info without the password hash
            return Ok(new
            {
                message = "Login successful",
                userId = foundUser.UserId,
                fullName = foundUser.FullName,
                email = foundUser.Email,
                role = foundUser.Role
            });
        }

        // handles new user registration
        [HttpPost("register")]
        public IActionResult Register([FromBody] User request)
        {
            // check if fields are empty
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.PasswordHash) ||
                string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(new { message = "All fields are required" });
            }

            // hash the password before saving (Week 4 - never store plain text)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash);

            // save the new user to the database
            using (SqlConnection connection = _db.CreateConnection())
            {
                // open the connection
                connection.Open();

                // use parameterized query to prevent sql injection (Week 3)
                string query = "INSERT INTO Users (full_name, email, password_hash, role) VALUES (@FullName, @Email, @PasswordHash, @Role)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // add all parameters safely
                    command.Parameters.AddWithValue("@FullName", request.FullName);
                    command.Parameters.AddWithValue("@Email", request.Email);
                    command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    command.Parameters.AddWithValue("@Role", "user");

                    // run the insert command
                    command.ExecuteNonQuery();
                }
            }

            // log the registration (Week 12)
            LogAction(null, "New user registered: " + request.Email, GetIpAddress(), "success");

            // return success message
            return Ok(new { message = "Registration successful" });
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