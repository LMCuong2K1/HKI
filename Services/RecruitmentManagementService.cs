using System;
using System.Collections.Generic;
using ConsoleApp1.Entities;
using MySql.Data.MySqlClient;

namespace ConsoleApp1.Services
{
    public class RecruitmentManagementService
    {
        private readonly string _connectionString;

        public RecruitmentManagementService()
        {
            _connectionString = DataAccess.DatabaseConfig.ConnectionString;
        }

        public bool AddJobPosting(JobPosting jobPosting)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string insertQuery = @"INSERT INTO JobPostings (JobPostingID, Title, Description, Requirements, Location, SalaryRange, DatePosted, ClosingDate, Status, PostedByAdminID)
                                       VALUES (@JobPostingID, @Title, @Description, @Requirements, @Location, @SalaryRange, @DatePosted, @ClosingDate, @Status, @PostedByAdminID)";
                using (var cmd = new MySqlCommand(insertQuery, connection))
                {
                    jobPosting.JobPostingID = Guid.NewGuid().ToString();
                    jobPosting.DatePosted = DateTime.Now;

                    cmd.Parameters.AddWithValue("@JobPostingID", jobPosting.JobPostingID);
                    cmd.Parameters.AddWithValue("@Title", jobPosting.Title);
                    cmd.Parameters.AddWithValue("@Description", jobPosting.Description);
                    cmd.Parameters.AddWithValue("@Requirements", jobPosting.Requirements);
                    cmd.Parameters.AddWithValue("@Location", jobPosting.Location);
                    cmd.Parameters.AddWithValue("@SalaryRange", jobPosting.SalaryRange);
                    cmd.Parameters.AddWithValue("@DatePosted", jobPosting.DatePosted);
                    cmd.Parameters.AddWithValue("@ClosingDate", jobPosting.ClosingDate.HasValue ? (object)jobPosting.ClosingDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", jobPosting.Status.ToString());
                    cmd.Parameters.AddWithValue("@PostedByAdminID", jobPosting.PostedByAdminID);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public JobApplication GetApplicationById(string applicationId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM JobApplications WHERE ApplicationID = @ApplicationID";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new JobApplication
                            {
                                ApplicationID = reader["ApplicationID"].ToString(),
                                JobPostingID = reader["JobPostingID"].ToString(),
                                CandidateID = reader["CandidateID"].ToString(),
                                SubmissionDate = Convert.ToDateTime(reader["SubmissionDate"]),
                                ResumeLink = reader["ResumeLink"].ToString(),
                                CoverLetter = reader["CoverLetter"].ToString(),
                                Status = Enum.TryParse(reader["Status"].ToString(), out ApplicationStatus status) ? status : ApplicationStatus.RECEIVED,
                                ReviewNotes = reader["ReviewNotes"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        public JobPosting GetJobPostingById(string jobPostingId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM JobPostings WHERE JobPostingID = @JobPostingID";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@JobPostingID", jobPostingId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new JobPosting
                            {
                                JobPostingID = reader["JobPostingID"].ToString(),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                Requirements = reader["Requirements"].ToString(),
                                Location = reader["Location"].ToString(),
                                SalaryRange = reader["SalaryRange"].ToString(),
                                DatePosted = Convert.ToDateTime(reader["DatePosted"]),
                                ClosingDate = reader["ClosingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ClosingDate"]),
                                Status = Enum.TryParse(reader["Status"].ToString(), out JobPostingStatus status) ? status : JobPostingStatus.OPEN,
                                PostedByAdminID = reader["PostedByAdminID"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool UpdateJobPosting(JobPosting jobPosting)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string updateQuery = @"UPDATE JobPostings SET Title = @Title, Description = @Description, Requirements = @Requirements, Location = @Location, SalaryRange = @SalaryRange, ClosingDate = @ClosingDate, Status = @Status WHERE JobPostingID = @JobPostingID";
                using (var cmd = new MySqlCommand(updateQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@JobPostingID", jobPosting.JobPostingID);
                    cmd.Parameters.AddWithValue("@Title", jobPosting.Title);
                    cmd.Parameters.AddWithValue("@Description", jobPosting.Description);
                    cmd.Parameters.AddWithValue("@Requirements", jobPosting.Requirements);
                    cmd.Parameters.AddWithValue("@Location", jobPosting.Location);
                    cmd.Parameters.AddWithValue("@SalaryRange", jobPosting.SalaryRange);
                    cmd.Parameters.AddWithValue("@ClosingDate", jobPosting.ClosingDate.HasValue ? (object)jobPosting.ClosingDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", jobPosting.Status.ToString());

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public bool DeleteJobPosting(string jobPostingId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string deleteQuery = @"DELETE FROM JobPostings WHERE JobPostingID = @JobPostingID";
                using (var cmd = new MySqlCommand(deleteQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@JobPostingID", jobPostingId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public List<Entities.PendingJobApplicationDto> GetPendingApplications()
        {
            var pendingApplications = new List<Entities.PendingJobApplicationDto>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT ja.ApplicationID, ja.JobPostingID, ja.CandidateID, ja.SubmissionDate, ja.ResumeLink, ja.CoverLetter, ja.Status, ja.ReviewNotes,
                           u.FullName AS CandidateName, u.Email AS CandidateEmail, u.Phone AS CandidatePhone,
                           jp.Title AS PositionName,
                           ja.ResumeLink AS CVLink
                    FROM JobApplications ja
                    INNER JOIN Users u ON ja.CandidateID = u.UserID
                    INNER JOIN JobPostings jp ON ja.JobPostingID = jp.JobPostingID
                    WHERE ja.Status = @Status";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Status", ApplicationStatus.RECEIVED.ToString());
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var application = new Entities.PendingJobApplicationDto
                            {
                                ApplicationID = reader["ApplicationID"].ToString(),
                                JobPostingID = reader["JobPostingID"].ToString(),
                                CandidateID = reader["CandidateID"].ToString(),
                                SubmissionDate = Convert.ToDateTime(reader["SubmissionDate"]),
                                ResumeLink = reader["ResumeLink"].ToString(),
                                CoverLetter = reader["CoverLetter"].ToString(),
                                Status = Enum.TryParse(reader["Status"].ToString(), out ApplicationStatus status) ? status : ApplicationStatus.RECEIVED,
                                ReviewNotes = reader["ReviewNotes"].ToString(),
                                CandidateName = reader["CandidateName"].ToString(),
                                CandidateEmail = reader["CandidateEmail"].ToString(),
                                CandidatePhone = reader["CandidatePhone"].ToString(),
                                PositionName = reader["PositionName"].ToString(),
                                CVLink = reader["CVLink"].ToString()
                            };
                            pendingApplications.Add(application);
                        }
                    }
                }
            }
            return pendingApplications;
        }

        public List<JobPosting> GetAllJobPostings()
        {
            var jobPostings = new List<JobPosting>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM JobPostings";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var jobPosting = new JobPosting
                            {
                                JobPostingID = reader["JobPostingID"].ToString(),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                Requirements = reader["Requirements"].ToString(),
                                Location = reader["Location"].ToString(),
                                SalaryRange = reader["SalaryRange"].ToString(),
                                DatePosted = Convert.ToDateTime(reader["DatePosted"]),
                                ClosingDate = reader["ClosingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ClosingDate"]),
                                Status = Enum.TryParse(reader["Status"].ToString(), out JobPostingStatus status) ? status : JobPostingStatus.OPEN,
                                PostedByAdminID = reader["PostedByAdminID"].ToString()
                            };
                            jobPostings.Add(jobPosting);
                        }
                    }
                }
            }
            return jobPostings;
        }

        public bool SubmitJobApplication(JobApplication application)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string insertQuery = @"INSERT INTO JobApplications (ApplicationID, JobPostingID, CandidateID, SubmissionDate, ResumeLink, CoverLetter, Status, ReviewNotes)
                                       VALUES (@ApplicationID, @JobPostingID, @CandidateID, @SubmissionDate, @ResumeLink, @CoverLetter, @Status, @ReviewNotes)";
                using (var cmd = new MySqlCommand(insertQuery, connection))
                {
                    application.ApplicationID = Guid.NewGuid().ToString();
                    application.SubmissionDate = DateTime.Now;

                    cmd.Parameters.AddWithValue("@ApplicationID", application.ApplicationID);
                    cmd.Parameters.AddWithValue("@JobPostingID", application.JobPostingID);
                    cmd.Parameters.AddWithValue("@CandidateID", application.CandidateID);
                    cmd.Parameters.AddWithValue("@SubmissionDate", application.SubmissionDate);
                    cmd.Parameters.AddWithValue("@ResumeLink", application.ResumeLink);
                    cmd.Parameters.AddWithValue("@CoverLetter", application.CoverLetter);
                    cmd.Parameters.AddWithValue("@Status", application.Status.ToString());
                    cmd.Parameters.AddWithValue("@ReviewNotes", application.ReviewNotes);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public bool UpdateApplicationStatus(string applicationId, ApplicationStatus newStatus, string reviewNotes = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string updateQuery = @"UPDATE JobApplications SET Status = @Status, ReviewNotes = @ReviewNotes WHERE ApplicationID = @ApplicationID";
                using (var cmd = new MySqlCommand(updateQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus.ToString());
                    cmd.Parameters.AddWithValue("@ReviewNotes", reviewNotes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ApplicationID", applicationId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public List<JobApplication> GetApplicationsByCandidate(string candidateId)
        {
            var applications = new List<JobApplication>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM JobApplications WHERE CandidateID = @CandidateID";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@CandidateID", candidateId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var application = new JobApplication
                            {
                                ApplicationID = reader["ApplicationID"].ToString(),
                                JobPostingID = reader["JobPostingID"].ToString(),
                                CandidateID = reader["CandidateID"].ToString(),
                                SubmissionDate = Convert.ToDateTime(reader["SubmissionDate"]),
                                ResumeLink = reader["ResumeLink"].ToString(),
                                CoverLetter = reader["CoverLetter"].ToString(),
                                Status = Enum.TryParse(reader["Status"].ToString(), out ApplicationStatus status) ? status : ApplicationStatus.RECEIVED,
                                ReviewNotes = reader["ReviewNotes"].ToString()
                            };
                            applications.Add(application);
                        }
                    }
                }
            }
            return applications;
        }

        public List<JobApplication> GetApplicationsByStatus(string status)
        {
            var applications = new List<JobApplication>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM JobApplications WHERE Status = @Status";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var application = new JobApplication
                            {
                                ApplicationID = reader["ApplicationID"].ToString(),
                                JobPostingID = reader["JobPostingID"].ToString(),
                                CandidateID = reader["CandidateID"].ToString(),
                                SubmissionDate = Convert.ToDateTime(reader["SubmissionDate"]),
                                ResumeLink = reader["ResumeLink"].ToString(),
                                CoverLetter = reader["CoverLetter"].ToString(),
                                Status = Enum.TryParse(reader["Status"].ToString(), out ApplicationStatus appStatus) ? appStatus : ApplicationStatus.RECEIVED,
                                ReviewNotes = reader["ReviewNotes"].ToString()
                            };
                            applications.Add(application);
                        }
                    }
                }
            }
            return applications;
        }
    }
}
