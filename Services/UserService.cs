using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ConsoleApp1.Entities;
using MySql.Data.MySqlClient;

namespace ConsoleApp1.Services
{
    public class UserService
    {
        private readonly string _connectionString;

        public UserService()
        {
            _connectionString = DataAccess.DatabaseConfig.ConnectionString;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public bool RegisterUser(User newUser)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Check if email already exists
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                using (var checkCmd = new MySqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Email", newUser.Email);
                    long count = (long)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        return false; // Email already exists
                    }
                }

                // Insert new user
                string insertQuery = @"INSERT INTO Users (UserID, FullName, Email, Phone, PasswordHash, UserRole, RegistrationDate, IsActive)
                                       VALUES (@UserID, @FullName, @Email, @Phone, @PasswordHash, @UserRole, @RegistrationDate, @IsActive)";
                using (var insertCmd = new MySqlCommand(insertQuery, connection))
                {
                    newUser.UserID = Guid.NewGuid().ToString();
                    newUser.RegistrationDate = DateTime.Now;
                    newUser.IsActive = true;

                    insertCmd.Parameters.AddWithValue("@UserID", newUser.UserID);
                    insertCmd.Parameters.AddWithValue("@FullName", newUser.FullName);
                    insertCmd.Parameters.AddWithValue("@Email", newUser.Email);
                    insertCmd.Parameters.AddWithValue("@Phone", newUser.Phone);
                    insertCmd.Parameters.AddWithValue("@PasswordHash", HashPassword(newUser.PasswordHash));
                    insertCmd.Parameters.AddWithValue("@UserRole", newUser.Role.ToString());
                    insertCmd.Parameters.AddWithValue("@RegistrationDate", newUser.RegistrationDate);
                    insertCmd.Parameters.AddWithValue("@IsActive", newUser.IsActive);

                    int rowsAffected = insertCmd.ExecuteNonQuery();

                    // If the user is an admin, insert into Admins table to satisfy foreign key constraint
                    if (newUser.Role == UserRole.ADMIN)
                    {
                        string insertAdminQuery = "INSERT INTO Admins (UserID) VALUES (@UserID)";
                        using (var cmdAdmin = new MySqlCommand(insertAdminQuery, connection))
                        {
                            cmdAdmin.Parameters.AddWithValue("@UserID", newUser.UserID);
                            cmdAdmin.ExecuteNonQuery();
                        }
                    }

                    // If the user is a candidate, insert into Candidates table to satisfy foreign key constraint
                    if (newUser.Role == UserRole.CANDIDATE)
                    {
                        string insertCandidateQuery = "INSERT INTO Candidates (UserID) VALUES (@UserID)";
                        using (var cmdCandidate = new MySqlCommand(insertCandidateQuery, connection))
                        {
                            cmdCandidate.Parameters.AddWithValue("@UserID", newUser.UserID);
                            cmdCandidate.ExecuteNonQuery();
                        }
                    }

                    // If the user is a customer, insert into Customers table to satisfy foreign key constraint
                    if (newUser.Role == UserRole.CUSTOMER)
                    {
                        string insertCustomerQuery = "INSERT INTO Customers (UserID) VALUES (@UserID)";
                        using (var cmdCustomer = new MySqlCommand(insertCustomerQuery, connection))
                        {
                            cmdCustomer.Parameters.AddWithValue("@UserID", newUser.UserID);
                            cmdCustomer.ExecuteNonQuery();
                        }
                    }

                    return rowsAffected > 0;
                }
            }
        }

        public void InsertMissingCustomers()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Insert into Customers table all users with role CUSTOMER who are not yet in Customers table
                string insertMissingQuery = @"
                    INSERT INTO Customers (UserID)
                    SELECT UserID FROM Users
                    WHERE UserRole = 'CUSTOMER' AND UserID NOT IN (SELECT UserID FROM Customers)";
                using (var cmd = new MySqlCommand(insertMissingQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public User AuthenticateUser(string email, string password)
        {
            User user = null;
            string hashedPassword = HashPassword(password);
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Users WHERE Email = @Email AND PasswordHash = @PasswordHash AND IsActive = 1";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                UserID = reader["UserID"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                Role = Enum.TryParse(reader["UserRole"].ToString(), out UserRole role) ? role : UserRole.CUSTOMER,
                                RegistrationDate = Convert.ToDateTime(reader["RegistrationDate"]),
                                LastLoginDate = reader["LastLoginDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LastLoginDate"]),
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            };
                        }
                    }
                }
            }
            return user;
        }

        public User GetUserById(string userId)
        {
            User user = null;
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Users WHERE UserID = @UserID";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                UserID = reader["UserID"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                Role = Enum.TryParse(reader["UserRole"].ToString(), out UserRole role) ? role : UserRole.CUSTOMER,
                                RegistrationDate = Convert.ToDateTime(reader["RegistrationDate"]),
                                LastLoginDate = reader["LastLoginDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LastLoginDate"]),
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            };
                        }
                    }
                }
            }
            return user;
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Users";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new User
                            {
                                UserID = reader["UserID"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                Role = Enum.TryParse(reader["UserRole"].ToString(), out UserRole role) ? role : UserRole.CUSTOMER,
                                RegistrationDate = Convert.ToDateTime(reader["RegistrationDate"]),
                                LastLoginDate = reader["LastLoginDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LastLoginDate"]),
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            };
                            users.Add(user);
                        }
                    }
                }
            }
            return users;
        }

        public bool UpdateUserRole(string userId, UserRole newRole, Action<MySqlConnection, MySqlTransaction> postUpdateAction = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Get current role
                string getRoleQuery = "SELECT UserRole FROM Users WHERE UserID = @UserID";
                string currentRole = null;
                using (var getRoleCmd = new MySqlCommand(getRoleQuery, connection))
                {
                    getRoleCmd.Parameters.AddWithValue("@UserID", userId);
                    var result = getRoleCmd.ExecuteScalar();
                    if (result != null)
                    {
                        currentRole = result.ToString();
                        Console.WriteLine($"Current role for user {userId} is {currentRole}");
                    }
                    else
                    {
                        Console.WriteLine($"User with ID {userId} not found.");
                        return false;
                    }
                }

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Update role in Users table
                        string updateQuery = "UPDATE Users SET UserRole = @UserRole WHERE UserID = @UserID";
                        using (var cmd = new MySqlCommand(updateQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserRole", newRole.ToString());
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                Console.WriteLine($"Failed to update role for user ID {userId}.");
                                transaction.Rollback();
                                return false;
                            }
                            else
                            {
                                Console.WriteLine($"Updated role to {newRole} for user {userId}");
                            }
                        }

                        // Handle role-specific tables
                        // Remove from old role table if applicable
                        if (currentRole == "ADMIN")
                        {
                            string deleteAdmin = "DELETE FROM Admins WHERE UserID = @UserID";
                            using (var cmd = new MySqlCommand(deleteAdmin, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else if (currentRole == "CANDIDATE")
                        {
                            // Update JobApplications to nullify CandidateID before deleting candidate to avoid FK constraint
                            string updateJobApplications = "UPDATE JobApplications SET CandidateID = NULL WHERE CandidateID = @UserID";
                            using (var cmdUpdate = new MySqlCommand(updateJobApplications, connection, transaction))
                            {
                                cmdUpdate.Parameters.AddWithValue("@UserID", userId);
                                cmdUpdate.ExecuteNonQuery();
                            }

                            string deleteCandidate = "DELETE FROM Candidates WHERE UserID = @UserID";
                            using (var cmd = new MySqlCommand(deleteCandidate, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else if (currentRole == "CUSTOMER")
                        {
                            string deleteCustomer = "DELETE FROM Customers WHERE UserID = @UserID";
                            using (var cmd = new MySqlCommand(deleteCustomer, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else if (currentRole == "BARBER")
                        {
                            string deleteBarber = "DELETE FROM Barbers WHERE UserID = @UserID";
                            using (var cmd = new MySqlCommand(deleteBarber, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Insert into new role table if applicable
                        if (newRole == UserRole.ADMIN)
                        {
                            string insertAdmin = "INSERT INTO Admins (UserID) VALUES (@UserID)";
                            try
                            {
                                using (var cmd = new MySqlCommand(insertAdmin, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@UserID", userId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error inserting into Admins table: {ex.Message}");
                                throw;
                            }
                        }
                        else if (newRole == UserRole.CANDIDATE)
                        {
                            string insertCandidate = "INSERT INTO Candidates (UserID) VALUES (@UserID)";
                            try
                            {
                                using (var cmd = new MySqlCommand(insertCandidate, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@UserID", userId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error inserting into Candidates table: {ex.Message}");
                                throw;
                            }
                        }
                        else if (newRole == UserRole.CUSTOMER)
                        {
                            string insertCustomer = "INSERT INTO Customers (UserID) VALUES (@UserID)";
                            try
                            {
                                using (var cmd = new MySqlCommand(insertCustomer, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@UserID", userId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error inserting into Customers table: {ex.Message}");
                                throw;
                            }
                        }
                        else if (newRole == UserRole.BARBER)
                        {
                            string insertBarber = "INSERT INTO Barbers (UserID) VALUES (@UserID)";
                            try
                            {
                                using (var cmd = new MySqlCommand(insertBarber, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@UserID", userId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error inserting into Barbers table: {ex.Message}");
                                throw;
                            }
                        }

                        // Execute additional action inside the same transaction before commit
                        if (postUpdateAction != null)
                        {
                            try
                            {
                                postUpdateAction.Invoke(connection, transaction);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Exception in postUpdateAction: {ex.Message}");
                                transaction.Rollback();
                                throw;
                            }
                        }

                        transaction.Commit();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception in UpdateUserRole: {ex.Message}");
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        // Modified method to update user details with optional password update
        public bool UpdateUserDetails(User user, string newPassword = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Check if email is used by another user
                string checkEmailQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserID != @UserID";
                using (var checkCmd = new MySqlCommand(checkEmailQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Email", user.Email);
                    checkCmd.Parameters.AddWithValue("@UserID", user.UserID);
                    long count = (long)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        Console.WriteLine("Email da duoc su dung boi nguoi dung khac.");
                        return false;
                    }
                }

                string updateQuery;
                if (string.IsNullOrEmpty(newPassword))
                {
                    updateQuery = @"UPDATE Users SET FullName = @FullName, Email = @Email, Phone = @Phone WHERE UserID = @UserID";
                }
                else
                {
                    updateQuery = @"UPDATE Users SET FullName = @FullName, Email = @Email, Phone = @Phone, PasswordHash = @PasswordHash WHERE UserID = @UserID";
                }

                using (var updateCmd = new MySqlCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.AddWithValue("@FullName", user.FullName);
                    updateCmd.Parameters.AddWithValue("@Email", user.Email);
                    updateCmd.Parameters.AddWithValue("@Phone", user.Phone);
                    updateCmd.Parameters.AddWithValue("@UserID", user.UserID);

                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        updateCmd.Parameters.AddWithValue("@PasswordHash", HashPassword(newPassword));
                    }

                    int rowsAffected = updateCmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        // New method to delete user and remove from role-specific tables
        public bool DeleteUser(string userId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Get user role
                string getRoleQuery = "SELECT UserRole FROM Users WHERE UserID = @UserID";
                string role = null;
                using (var getRoleCmd = new MySqlCommand(getRoleQuery, connection))
                {
                    getRoleCmd.Parameters.AddWithValue("@UserID", userId);
                    var result = getRoleCmd.ExecuteScalar();
                    if (result != null)
                    {
                        role = result.ToString();
                    }
                    else
                    {
                        Console.WriteLine("User not found.");
                        return false;
                    }
                }

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Remove from role-specific tables
                        if (role == "ADMIN")
                        {
                            string deleteAdmin = "DELETE FROM Admins WHERE UserID = @UserID";
                            using (var cmd = new MySqlCommand(deleteAdmin, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else if (role == "CANDIDATE")
                        {
                            // Update JobApplications to nullify CandidateID before deleting candidate to avoid FK constraint
                            string updateJobApplications = "UPDATE JobApplications SET CandidateID = NULL WHERE CandidateID = @UserID";
                            using (var cmdUpdate = new MySqlCommand(updateJobApplications, connection, transaction))
                            {
                                cmdUpdate.Parameters.AddWithValue("@UserID", userId);
                                cmdUpdate.ExecuteNonQuery();
                            }

                            string deleteCandidate = "DELETE FROM Candidates WHERE UserID = @UserID";
                            using (var cmd = new MySqlCommand(deleteCandidate, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else if (role == "CUSTOMER")
                        {
                            string deleteCustomer = "DELETE FROM Customers WHERE UserID = @UserID";
                            using (var cmd = new MySqlCommand(deleteCustomer, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else if (role == "BARBER")
                        {
                            string deleteBarber = "DELETE FROM Barbers WHERE UserID = @UserID";
                            using (var cmd = new MySqlCommand(deleteBarber, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Delete from Users table
                        string deleteUser = "DELETE FROM Users WHERE UserID = @UserID";
                        using (var cmdDeleteUser = new MySqlCommand(deleteUser, connection, transaction))
                        {
                            cmdDeleteUser.Parameters.AddWithValue("@UserID", userId);
                            int rows = cmdDeleteUser.ExecuteNonQuery();
                            if (rows == 0)
                            {
                                Console.WriteLine("Failed to delete user.");
                                transaction.Rollback();
                                return false;
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception in DeleteUser: {ex.Message}");
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
    }
}
