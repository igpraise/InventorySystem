// File: Log.cs
// Student: Chinonso Praise Ignatius
// Course: SECU2000 - Application Security
// Description: this is the log model, it matches the Logs table in the database
//              logs record every important action in the system (Week 12)

namespace InventoryAPI.Models
{
    // this class represents a log entry in the system
    public class Log
    {
        // unique id for each log entry
        public int LogId { get; set; }

        // which user did the action, can be null if not logged in
        public int? UserId { get; set; }

        // what action was performed
        public string Action { get; set; } = string.Empty;

        // ip address of the user who did the action
        public string IpAddress { get; set; } = string.Empty;

        // whether the action was success or failure
        public string Status { get; set; } = string.Empty;

        // when the action happened
        public DateTime LoggedAt { get; set; }
    }

    // this class is used when we want to create a new log entry
    public class LogRequest
    {
        // which user did the action
        public int? UserId { get; set; }

        // what action was performed
        public string Action { get; set; } = string.Empty;

        // ip address of the user
        public string IpAddress { get; set; } = string.Empty;

        // whether it was success or failure
        public string Status { get; set; } = string.Empty;
    }
}