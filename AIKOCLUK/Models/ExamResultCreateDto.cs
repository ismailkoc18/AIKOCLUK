namespace AIKOCLUK.Models
{
    public class ExamResultCreateDto
    {
        public string ExamType { get; set; } = "TYT";
        public DateTime ExamDate { get; set; } = DateTime.UtcNow;
        public double TurkishNet { get; set; }
        public double MathNet { get; set; }
        public double ScienceNet { get; set; }
        public double SocialNet { get; set; }
        public bool TimeManagementIssue { get; set; }
    }
}