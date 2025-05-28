using System;

namespace ConsoleApp1.Entities
{
    public enum ApplicationStatus
    {
        RECEIVED,
        UNDER_REVIEW,
        INTERVIEW_SCHEDULED,
        INTERVIEW_COMPLETED,
        OFFERED,
        REJECTED,
        HIRED,
        WITHDRAWN
    }

    public class JobApplication
    {
        public string ApplicationID { get; set; }
        public string JobPostingID { get; set; }
        public string CandidateID { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string ResumeLink { get; set; }
        public string CoverLetter { get; set; }
        public ApplicationStatus Status { get; set; }
        public string ReviewNotes { get; set; }
    }
}
