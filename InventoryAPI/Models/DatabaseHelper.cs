// File: DatabaseHelper.cs
// Student: Chinonso Praise Ignatius
// Course: SECU2000 - Application Security
// Description: handles the database connection for the app

using Microsoft.Data.SqlClient;

namespace InventoryAPI.Models
{
    // this class creates a connection to the sql server database
    public class DatabaseHelper
    {
        // store the connection string
        private readonly string _connectionString;

        // constructor - gets the connection string from appsettings.json
        public DatabaseHelper(IConfiguration configuration)
        {
            // read the connection string from config file
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        // creates and returns a new database connection
        public SqlConnection CreateConnection()
        {
            // return a new sql connection using our connection string
            return new SqlConnection(_connectionString);
        }
    }
}