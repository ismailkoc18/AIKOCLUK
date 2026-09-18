using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIKOCLUK.Models
{
    public class AiFeedback
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        // AI'ın verdiği tavsiye/program
        [Required]
        public string GeneratedAdvice { get; set; } = string.Empty;

        // Öğrencinin AI'a yazdığı orijinal soru/durum mesajı
        public string StudentNote { get; set; } = string.Empty;

        // AI'ın öğrencinin mesajından çıkardığı anlık duygu durumu (Örn: "Stresli", "Motivasyonlu")
        public string DetectedSentiment { get; set; } = string.Empty;
    }
}