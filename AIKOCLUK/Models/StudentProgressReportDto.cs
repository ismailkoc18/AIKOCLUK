namespace AIKOCLUK.Models
{
    // Ders bazlı son 5 deneme analiz modeli
    public class SubjectProgressDto
    {
        public string SubjectName { get; set; } = string.Empty;
        public double AverageNet { get; set; }        // Son 5 denemedeki ortalama net
        public double OldestExamNet { get; set; }     // Son 5 denemenin ilkindeki net
        public double LatestExamNet { get; set; }     // En son denemedeki net
        public double NetChange { get; set; }         // Değişim miktarı (Son Deneme - İlk Deneme)
        public string StatusTrend { get; set; } = string.Empty; // Yükselişte, Düşüşte, Stabil
    }

    // Son 5 denemede en çok tekrarlanan kritik konu eksikleri
    public class TopWeakTopicDto
    {
        public string Subject { get; set; } = string.Empty;
        public string TopicName { get; set; } = string.Empty;
        public int TotalErrors { get; set; } // Son 5 denemedeki toplam yanlış + boş sayısı
    }

    // Grafik veya liste gösterimi için son 5 denemenin kısa özeti
    public class ExamSummaryDto
    {
        public int ExamId { get; set; }
        public string ExamType { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public double TotalNet { get; set; }
    }

    // Öğrenciye sunulacak genel son 5 deneme analiz raporu
    public class StudentProgressReportDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int ExamCountAnalyzed { get; set; } // Analiz edilen deneme sayısı (Örn: 5 veya henüz 5 olmadıysa mevcut sayı)
        public DateTime? PeriodStartDate { get; set; } // Analiz edilen ilk deneme tarihi
        public DateTime? PeriodEndDate { get; set; }   // Analiz edilen son deneme tarihi
        public List<SubjectProgressDto> SubjectProgresses { get; set; } = new();
        public List<TopWeakTopicDto> CriticalWeakTopics { get; set; } = new();
        public List<ExamSummaryDto> RecentExamsSummary { get; set; } = new();
    }
}