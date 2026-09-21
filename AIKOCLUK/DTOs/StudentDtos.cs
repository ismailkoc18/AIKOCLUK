namespace AIKOCLUK.DTOs
{
    public class StudentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TargetUniversity { get; set; } = string.Empty;
        public string TargetDepartment { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty; // SAY, EA, SÖZ, DİL
        public string LearningStyle { get; set; } = string.Empty;
        public int DailyAvailableStudyHours { get; set; }
        public int CurrentStressLevel { get; set; }
        public string WeakSubjects { get; set; } = string.Empty;
    }

    public class StudentUpdateDto
    {
        public string TargetUniversity { get; set; } = string.Empty;
        public string TargetDepartment { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string LearningStyle { get; set; } = string.Empty;
        public int DailyAvailableStudyHours { get; set; }
        public int CurrentStressLevel { get; set; }
        public string WeakSubjects { get; set; } = string.Empty;
    }
}