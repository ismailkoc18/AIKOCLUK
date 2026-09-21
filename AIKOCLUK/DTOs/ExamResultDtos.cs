namespace AIKOCLUK.DTOs
{
    public class ExamResultResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string ExamType { get; set; } = string.Empty; // TYT, AYT
        public DateTime ExamDate { get; set; }
        public double TurkishNet { get; set; }
        public double MathNet { get; set; }
        public double ScienceNet { get; set; }
        public double SocialNet { get; set; }
        public bool TimeManagementIssue { get; set; }
        public double TotalNet => TurkishNet + MathNet + ScienceNet + SocialNet;
    }

    public class ExamResultCreateDto
    {
        public int StudentId { get; set; }
        public string ExamType { get; set; } = "TYT";
        public DateTime ExamDate { get; set; } = DateTime.UtcNow;
        public double TurkishNet { get; set; }
        public double MathNet { get; set; }
        public double ScienceNet { get; set; }
        public double SocialNet { get; set; }
        public bool TimeManagementIssue { get; set; }
    }

    public class ExamResultUpdateDto
    {
        public string ExamType { get; set; } = "TYT";
        public DateTime ExamDate { get; set; }
        public double TurkishNet { get; set; }
        public double MathNet { get; set; }
        public double ScienceNet { get; set; }
        public double SocialNet { get; set; }
        public bool TimeManagementIssue { get; set; }
    }
}