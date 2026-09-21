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

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Tavsiyenin üretildiği tarih

        public string GenelDegerlendirme { get; set; }

        public string HaftalikOdakTavsiyesi { get; set; }

        // SQLite'ta liste tutamadığımız için virgülle ayırıp kaydedeceğiz (Örn: "Matematik, Fizik")
        public string KirmiziAlarmDersleri { get; set; }

        // Bire-Çok İlişki (One-to-Many): Bir tavsiyenin altındaki hedef konular
        public List<AiTargetSubject> HedefKonular { get; set; } = new List<AiTargetSubject>();
    }

    // 2. Alt Tablo: Yapay zekanın çalışmasını önerdiği spesifik konular
    public class AiTargetSubject
    {
        [Key]
        public int Id { get; set; }

        public int AiAdviceHistoryId { get; set; } // Üst tablonun ID'si (Foreign Key)

        public string Ders { get; set; }
        public string Konu { get; set; }
        public string Neden { get; set; }
        public string Taktik { get; set; }
    }
}