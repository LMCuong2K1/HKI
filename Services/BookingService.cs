using System;
using System.Collections.Generic;
using ConsoleApp1.Entities;
using MySql.Data.MySqlClient;

namespace ConsoleApp1.Services
{
    public class BookingService
    {
        private readonly string _connectionString;

        public BookingService()
        {
            _connectionString = DataAccess.DatabaseConfig.ConnectionString;
        }

        public List<User> GetAvailableBarbers()
        {
            var barbers = new List<User>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Users WHERE UserRole = 'BARBER' AND IsActive = 1";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var barber = new User
                            {
                                UserID = reader["UserID"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                Role = UserRole.BARBER,
                                RegistrationDate = Convert.ToDateTime(reader["RegistrationDate"]),
                                LastLoginDate = reader["LastLoginDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LastLoginDate"]),
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            };
                            barbers.Add(barber);
                        }
                    }
                }
            }
            return barbers;
        }

        public List<Appointment> ViewAllAppointments()
        {
            var appointments = new List<Appointment>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"SELECT AppointmentID, SlotID, CustomerID, BarberID, ServiceID, AppointmentDateTime, Status, Notes, DurationMinutes, BookingDate
                                 FROM Appointments";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var appointment = new Appointment
                            {
                                AppointmentID = reader["AppointmentID"].ToString(),
                                SlotID = reader["SlotID"].ToString(),
                                CustomerID = reader["CustomerID"].ToString(),
                                BarberID = reader["BarberID"].ToString(),
                                ServiceID = reader["ServiceID"].ToString(),
                                AppointmentDateTime = Convert.ToDateTime(reader["AppointmentDateTime"]),
                                Status = Enum.TryParse(reader["Status"].ToString(), out AppointmentStatus status) ? status : AppointmentStatus.Pending,
                                Notes = reader["Notes"].ToString(),
                                Duration = TimeSpan.FromMinutes(Convert.ToDouble(reader["DurationMinutes"])),
                                BookingDate = Convert.ToDateTime(reader["BookingDate"])
                            };
                            appointments.Add(appointment);
                        }
                    }
                }
            }
            return appointments;
        }

        public bool UpdateAppointmentStatusAdmin(string appointmentId, AppointmentStatus newStatus)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"UPDATE Appointments SET Status = @Status WHERE AppointmentID = @AppointmentID";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus.ToString());
                    cmd.Parameters.AddWithValue("@AppointmentID", appointmentId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public bool CreateAvailabilitySlots(string barberId, DateTime date)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        bool result = CreateAvailabilitySlots(barberId, date, connection, transaction);
                        if (result)
                        {
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                        return result;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public bool CreateAvailabilitySlots(string barberId, DateTime date, MySqlConnection connection, MySqlTransaction transaction)
        {
            var slots = new List<(TimeSpan StartTime, TimeSpan EndTime)>();
            // Store hours: 8:00 to 12:00, break 12:00-13:00, then 13:00 to 19:00
            for (int hour = 8; hour < 12; hour++)
            {
                slots.Add((new TimeSpan(hour, 0, 0), new TimeSpan(hour + 1, 0, 0)));
            }
            for (int hour = 13; hour < 19; hour++)
            {
                slots.Add((new TimeSpan(hour, 0, 0), new TimeSpan(hour + 1, 0, 0)));
            }

            try
            {
                // Delete existing slots for the date
                string deleteQuery = @"DELETE FROM AvailabilitySlots WHERE BarberID = @BarberID AND SlotDate = @SlotDate";
                using (var deleteCmd = new MySqlCommand(deleteQuery, connection, transaction))
                {
                    deleteCmd.Parameters.AddWithValue("@BarberID", barberId);
                    deleteCmd.Parameters.AddWithValue("@SlotDate", date.Date);
                    deleteCmd.ExecuteNonQuery();
                }

                // Insert new slots
                string insertQuery = @"INSERT INTO AvailabilitySlots (SlotID, BarberID, SlotDate, StartTime, EndTime, IsAvailable)
                                       VALUES (@SlotID, @BarberID, @SlotDate, @StartTime, @EndTime, 1)";
                foreach (var slot in slots)
                {
                    using (var insertCmd = new MySqlCommand(insertQuery, connection, transaction))
                    {
                        insertCmd.Parameters.AddWithValue("@SlotID", Guid.NewGuid().ToString());
                        insertCmd.Parameters.AddWithValue("@BarberID", barberId);
                        insertCmd.Parameters.AddWithValue("@SlotDate", date.Date);
                        insertCmd.Parameters.AddWithValue("@StartTime", slot.StartTime);
                        insertCmd.Parameters.AddWithValue("@EndTime", slot.EndTime);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                // Do not commit here, commit is managed by caller
                Console.WriteLine($"Created {slots.Count} availability slots for barber {barberId} on {date.Date.ToShortDateString()}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create availability slots: {ex.Message}");
                return false;
            }
        }

        public List<DayOfWeek> GetWeeklyAvailability(string barberId)
        {
            var workingDays = new List<DayOfWeek>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"SELECT DayOfWeek FROM BarberWeeklyAvailability WHERE BarberID = @BarberID";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@BarberID", barberId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (Enum.TryParse(reader["DayOfWeek"].ToString(), out DayOfWeek day))
                            {
                                workingDays.Add(day);
                            }
                        }
                    }
                }
            }
            return workingDays;
        }

        public bool UpdateWeeklyAvailability(string barberId, List<DayOfWeek> workingDays)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Delete existing availability
                        string deleteQuery = @"DELETE FROM BarberWeeklyAvailability WHERE BarberID = @BarberID";
                        using (var deleteCmd = new MySqlCommand(deleteQuery, connection, transaction))
                        {
                            deleteCmd.Parameters.AddWithValue("@BarberID", barberId);
                            deleteCmd.ExecuteNonQuery();
                        }

                        // Insert new availability
                        string insertQuery = @"INSERT INTO BarberWeeklyAvailability (BarberID, DayOfWeek) VALUES (@BarberID, @DayOfWeek)";
                        foreach (var day in workingDays)
                        {
                            using (var insertCmd = new MySqlCommand(insertQuery, connection, transaction))
                            {
                                insertCmd.Parameters.AddWithValue("@BarberID", barberId);
                                insertCmd.Parameters.AddWithValue("@DayOfWeek", day.ToString());
                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public List<AvailabilitySlot> GetAvailableSlots(string barberId, DateTime date)
        {
            var slots = new List<AvailabilitySlot>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"SELECT * FROM AvailabilitySlots 
                                 WHERE BarberID = @BarberID AND SlotDate = @SlotDate AND IsAvailable = 1";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@BarberID", barberId);
                    cmd.Parameters.AddWithValue("@SlotDate", date.Date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var slot = new AvailabilitySlot
                            {
                                SlotID = reader["SlotID"].ToString(),
                                BarberID = reader["BarberID"].ToString(),
                                SlotDate = Convert.ToDateTime(reader["SlotDate"]),
                                StartTime = (TimeSpan)reader["StartTime"],
                                EndTime = (TimeSpan)reader["EndTime"],
                                IsAvailable = Convert.ToBoolean(reader["IsAvailable"])
                            };
                            slots.Add(slot);
                        }
                    }
                }
            }
            return slots;
        }

        public bool CreateNewAppointment(Appointment appointment)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // Check if slot is still available
                string checkSlotQuery = @"SELECT IsAvailable FROM AvailabilitySlots WHERE SlotID = @SlotID";
                using (var checkCmd = new MySqlCommand(checkSlotQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@SlotID", appointment.SlotID);
                    var isAvailableObj = checkCmd.ExecuteScalar();
                    if (isAvailableObj == null || !(bool)isAvailableObj)
                    {
                        return false; // Slot not available
                    }
                }

                // Check if barber is available on the appointment date
                string checkBarberAvailabilityQuery = @"SELECT COUNT(*) FROM BarberWeeklyAvailability 
                                                        WHERE BarberID = @BarberID AND DayOfWeek = @DayOfWeek";
                using (var checkBarberCmd = new MySqlCommand(checkBarberAvailabilityQuery, connection))
                {
                    checkBarberCmd.Parameters.AddWithValue("@BarberID", appointment.BarberID);
                    checkBarberCmd.Parameters.AddWithValue("@DayOfWeek", appointment.AppointmentDateTime.DayOfWeek.ToString());
                    var countObj = checkBarberCmd.ExecuteScalar();
                    int count = countObj != null ? Convert.ToInt32(countObj) : 0;
                    if (count == 0)
                    {
                        return false; // Barber not available on this day
                    }
                }

                // Insert appointment
                string insertQuery = @"INSERT INTO Appointments (AppointmentID, SlotID, CustomerID, BarberID, ServiceID, AppointmentDateTime, Status, Notes, DurationMinutes, BookingDate)
                                       VALUES (@AppointmentID, @SlotID, @CustomerID, @BarberID, @ServiceID, @AppointmentDateTime, @Status, @Notes, @DurationMinutes, @BookingDate)";
                using (var insertCmd = new MySqlCommand(insertQuery, connection))
                {
                    appointment.AppointmentID = Guid.NewGuid().ToString();
                    insertCmd.Parameters.AddWithValue("@AppointmentID", appointment.AppointmentID);
                    insertCmd.Parameters.AddWithValue("@SlotID", appointment.SlotID);
                    insertCmd.Parameters.AddWithValue("@CustomerID", appointment.CustomerID);
                    insertCmd.Parameters.AddWithValue("@BarberID", appointment.BarberID);
                    insertCmd.Parameters.AddWithValue("@ServiceID", appointment.ServiceID);
                    insertCmd.Parameters.AddWithValue("@AppointmentDateTime", appointment.AppointmentDateTime);
                    insertCmd.Parameters.AddWithValue("@Status", appointment.Status.ToString());
                    insertCmd.Parameters.AddWithValue("@Notes", appointment.Notes);
                    insertCmd.Parameters.AddWithValue("@DurationMinutes", appointment.Duration.TotalMinutes);
                    insertCmd.Parameters.AddWithValue("@BookingDate", appointment.BookingDate);

                    int rowsAffected = insertCmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        // Mark slot as booked
                        string updateSlotQuery = @"UPDATE AvailabilitySlots SET IsAvailable = 0 WHERE SlotID = @SlotID";
                        using (var updateCmd = new MySqlCommand(updateSlotQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@SlotID", appointment.SlotID);
                            updateCmd.ExecuteNonQuery();
                        }
                        return true;
                    }
                    return false;
                }
            }
        }

        // Additional methods like CancelUserAppointment, GetCustomerAppointments, etc. can be added similarly

        public bool HasAppointmentsForService(string serviceId)
        {
            using (var connection = new MySql.Data.MySqlClient.MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Appointments WHERE ServiceID = @ServiceID";
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ServiceID", serviceId);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
    }
}
