using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIKOCLUK.Models
{
    // 1. Ana Tablo: Öğrenciye verilen ana koçluk tavsiyesi
    public class AiAdviceHistory
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; } // Hangi öğrenciye ait olduğu
        public Student Student { get; set; } = null!; // Veritabanı ilişkisi (Navigation Property)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Tavsiyenin üretildiği tarih

        // --- Controller ve Service Yapısı İçin Eklenen Alanlar ---
        public string AdviceType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        // --- Mevcut Eski Alanların (Aynen Korundu) ---
        public string GenelDegerlendirme { get; set; } = string.Empty;

        public string HaftalikOdakTavsiyesi { get; set; } = string.Empty;

        // SQLite'ta liste tutamadığımız için virgülle ayırıp kaydedeceğiz (Örn: "Matematik, Fizik")
        public string KirmiziAlarmDersleri { get; set; } = string.Empty;

        // Bire-Çok İlişki (One-to-Many): Bir tavsiyenin altındaki hedef konular
        public List<AiTargetSubject> HedefKonular { get; set; } = new List<AiTargetSubject>();
    }

    // 2. Alt Tablo: Yapay zekanın çalışmasını önerdiği spesifik konular
    public class AiTargetSubject
    {
        [Key]
        public int Id { get; set; }

        public int AiAdviceHistoryId { get; set; } // Üst tablonun ID'si (Foreign Key)

        public string Ders { get; set; } = string.Empty;
        public string Konu { get; set; } = string.Empty;
        public string Neden { get; set; } = string.Empty;
        public string Taktik { get; set; } = string.Empty;
    }
}