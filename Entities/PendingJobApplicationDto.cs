using System;

namespace ConsoleApp1.Entities
{
    public class PendingJobApplicationDto
    {
        public string ApplicationID { get; set; }
        public string JobPostingID { get; set; }
        public string CandidateID { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string ResumeLink { get; set; }
        public string CoverLetter { get; set; }
        public ApplicationStatus Status { get; set; }
        public string ReviewNotes { get; set; }

        // Additional fields for display
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public string CandidatePhone { get; set; }
        public string PositionName { get; set; }
        public string CVLink { get; set; }
    }
}
