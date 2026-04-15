// File: User.cs
// Student: Chinonso Praise Ignatius
// Course: SECU2000 - Application Security
// Description: this is the user model, it matches the Users table in the database

namespace InventoryAPI.Models
{
    // this class represents a user in the system
    public class User
    {
        // unique id for each user
        public int UserId { get; set; }

        // full name of the user
        public string FullName { get; set; } = string.Empty;

        // email is used to login
        public string Email { get; set; } = string.Empty;

        // password is always stored as a hash never plain text (Week 4)
        public string PasswordHash { get; set; } = string.Empty;

        // role is either user or admin
        public string Role { get; set; } = "user";

        // tells us if the account is active or disabled
        public bool IsActive { get; set; } = true;

        // when the account was created
        public DateTime CreatedAt { get; set; }
    }

    // this class is used when someone tries to login
    public class LoginRequest
    {
        // email they type in the login form
        public string Email { get; set; } = string.Empty;

        // password they type in the login form
        public string Password { get; set; } = string.Empty;
    }
}