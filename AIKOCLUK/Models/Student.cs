using System.ComponentModel.DataAnnotations;

namespace AIKOCLUK.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // --- Akademik Hedefler ---
        public string TargetUniversity { get; set; } = string.Empty;
        public string TargetDepartment { get; set; } = string.Empty; // Örn: "Bilgisayar Mühendisliği"
        public string Field { get; set; } = string.Empty; // SAY, EA, SÖZ, DİL

        // --- Yapay Zekayı Kişiselleştirecek Profil Verileri ---
        public string LearningStyle { get; set; } = string.Empty; // Örn: "Görsel", "İşitsel"
        public int DailyAvailableStudyHours { get; set; }

        [Range(1, 10)]
        public int CurrentStressLevel { get; set; } // AI'ın dilini (empatik/disiplinli) belirleyecek

        public string WeakSubjects { get; set; } = string.Empty; // Örn: "Geometri, Fizik"

        // --- İlişkiler (Navigation Properties) ---
        public List<ExamResult> ExamResults { get; set; } = new();
        public List<AiFeedback> AiFeedbacks { get; set; } = new();
    }
}