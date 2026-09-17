namespace AIKOCLUK.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TargetDepartment { get; set; } = string.Empty; // Hedeflenen Bölüm (Örn: Bilgisayar Mühendisliği)
        public int DailyStudyHours { get; set; } // Günlük Çalışma Saati

        // İlişki: Bir öğrencinin birden çok deneme sonucu olabilir
        public List<ExamResult> ExamResults { get; set; } = new();
    }
}