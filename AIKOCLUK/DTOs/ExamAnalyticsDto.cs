namespace AIKOCLUK.DTOs
{
    // Tüm grafik verilerini tek bir yanıtta toplayan ana DTO
    public class DetailedExamAnalyticsDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public TrendChartDataDto NetTrendChart { get; set; } = new();
        public DoughnutChartDataDto ErrorBreakdownChart { get; set; } = new();
        public RadarChartDataDto SubjectMasteryChart { get; set; } = new();
        public BarChartDataDto TargetComparisonChart { get; set; } = new();
    }

    // 1. Çizgi Grafiği Verisi (Net Trendi)
    public class TrendChartDataDto
    {
        public List<string> Labels { get; set; } = new(); // Deneme Adları veya Tarihler (Örn: "Deneme 1", "Deneme 2")
        public List<ChartDatasetDto> Datasets { get; set; } = new();
    }

    // 2. Halka Grafik Verisi (Hata Nedenleri)
    public class DoughnutChartDataDto
    {
        public List<string> Labels { get; set; } = new List<string> { "Bilgi Eksikliği", "Süre Yetersizliği", "Dikkat Hatası" };
        public List<int> Values { get; set; } = new(); // Örn: [15, 8, 4]
        public List<double> Percentages { get; set; } = new(); // Örn: [55.5, 29.6, 14.8]
    }

    // 3. Radar Grafik Verisi (Konu Hakimiyeti)
    public class RadarChartDataDto
    {
        public List<string> SubjectLabels { get; set; } = new(); // Örn: ["Matematik", "Fizik", "Kimya", "Biyoloji"]
        public List<double> MasteryScores { get; set; } = new(); // 0 - 100 arası başarı yüzdesi
    }

    // 4. Bar Grafik Verisi (Hedef Karşılaştırma)
    public class BarChartDataDto
    {
        public List<string> Categories { get; set; } = new List<string> { "TYT Türkçe", "TYT Mat", "TYT Fen", "TYT Sosyal" };
        public List<double> CurrentAverages { get; set; } = new();
        public List<double> TargetAverages { get; set; } = new();
    }

    // Genel Veri Kümesi Yapısı
    public class ChartDatasetDto
    {
        public string Label { get; set; } = string.Empty; // Örn: "Matematik Netleri"
        public List<double> Data { get; set; } = new();
    }
}