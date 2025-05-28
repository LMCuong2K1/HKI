using System;

namespace ConsoleApp1.Entities
{
    public enum JobPostingStatus
    {
        OPEN,
        CLOSED,
        FILLED
    }

    public class JobPosting
    {
        public string JobPostingID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public string Location { get; set; }
        public string SalaryRange { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime? ClosingDate { get; set; }
        public JobPostingStatus Status { get; set; }
        public string PostedByAdminID { get; set; }
    }
}
