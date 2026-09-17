namespace AIKOCLUK.Models
{
    public class ExamResult
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public string ExamName { get; set; } = string.Empty; // Örn: TYT Genel Deneme 1
        public DateTime Date { get; set; } = DateTime.Now;

        // Net Bilgileri
        public double TurkishNet { get; set; }
        public double MathNet { get; set; }
        public double ScienceNet { get; set; }
        public double SocialNet { get; set; }

        // Yapay zekanın analiz edeceği zayıf/hatalı olunan alt konular (Virgülle ayrılmış veya metin)
        public string WeakTopics { get; set; } = string.Empty;
    }
}