using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIKOCLUK.Models
{
    public class ExamResult
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        [Required]
        public string ExamType { get; set; } = string.Empty; // "TYT" veya "AYT"
        public DateTime ExamDate { get; set; } = DateTime.UtcNow;

        // TYT Bazlı Örnek Ders Netleri
        public double TurkishNet { get; set; }
        public double MathNet { get; set; }
        public double ScienceNet { get; set; }
        public double SocialNet { get; set; }

        // Toplam neti veritabanında tutmak yerine dinamik hesaplatıyoruz
        [NotMapped]
        public double TotalNet => TurkishNet + MathNet + ScienceNet + SocialNet;

        // Yapay zeka analizi için kritik bir parametre
        public bool TimeManagementIssue { get; set; }

        // Bir denemenin birden fazla konu hatası olabileceğini belirten koleksiyon
        public List<ExamTopicError> TopicErrors { get; set; } = new();
    }
}