using System;

namespace ConsoleApp1.Entities
{
    public enum UserRole
    {
        CUSTOMER,
        ADMIN,
        BARBER,
        CANDIDATE
    }

    public class User
    {
        public string UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; }

        public bool VerifyPassword(string password)
        {
            // Implement password verification logic (e.g., hash comparison)
            return PasswordHash == password; // Placeholder, replace with real hash check
        }

        public void UpdateLastLogin()
        {
            LastLoginDate = DateTime.Now;
        }
    }

    public class Customer : User
    {
        public string MembershipLevel { get; set; }
        public string Address { get; set; }
    }

    public class Admin : User
    {
        // Additional admin-specific properties if needed
    }

    public class Barber : User
    {
        public string Specialty { get; set; }
        public int YearsOfExperience { get; set; }
        public float Rating { get; set; }
        public string PortfolioLink { get; set; }
    }

    public class Candidate : User
    {
        // Additional candidate-specific properties if needed
    }
}
