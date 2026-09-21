using AIKOCLUK.Data;
using AIKOCLUK.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AIKOCLUK.Services
{
    public class ExamAnalyticsService
    {
        private readonly AppDbContext _context;

        public ExamAnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DetailedExamAnalyticsDto> GetStudentAnalyticsAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.ExamResults)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new KeyNotFoundException("Öğrenci bulunamadı.");

            var exams = student.ExamResults.OrderBy(e => e.ExamDate).ToList();

            var analytics = new DetailedExamAnalyticsDto
            {
                StudentId = student.Id,
                StudentName = student.Name
            };

            // 1. Net Trend Grafiği
            analytics.NetTrendChart.Labels = exams.Select(e => e.ExamDate.ToString("dd MMM")).ToList();

            analytics.NetTrendChart.Datasets.Add(new ChartDatasetDto
            {
                Label = "Türkçe Net",
                Data = exams.Select(e => e.TurkishNet).ToList()
            });
            analytics.NetTrendChart.Datasets.Add(new ChartDatasetDto
            {
                Label = "Matematik Net",
                Data = exams.Select(e => e.MathNet).ToList()
            });
            analytics.NetTrendChart.Datasets.Add(new ChartDatasetDto
            {
                Label = "Fen Net",
                Data = exams.Select(e => e.ScienceNet).ToList()
            });
            analytics.NetTrendChart.Datasets.Add(new ChartDatasetDto
            {
                Label = "Sosyal Net",
                Data = exams.Select(e => e.SocialNet).ToList()
            });

            // 2. Hata Kök Neden Grafiği (Örnek Dağılım)
            int bilgiEksikligi = 18;
            int süreYetersizligi = 9;
            int dikkatHatasi = 5;
            int toplamHata = bilgiEksikligi + süreYetersizligi + dikkatHatasi;

            analytics.ErrorBreakdownChart.Values = new List<int> { bilgiEksikligi, süreYetersizligi, dikkatHatasi };
            if (toplamHata > 0)
            {
                analytics.ErrorBreakdownChart.Percentages = new List<double>
                {
                    Math.Round((double)bilgiEksikligi / toplamHata * 100, 1),
                    Math.Round((double)süreYetersizligi / toplamHata * 100, 1),
                    Math.Round((double)dikkatHatasi / toplamHata * 100, 1)
                };
            }

            // 3. Konu / Ders Hakimiyet Grafiği (Radar Chart)
            if (exams.Any())
            {
                analytics.SubjectMasteryChart.SubjectLabels = new List<string> { "Türkçe", "Matematik", "Fen Bilimleri", "Sosyal Bilimler" };
                analytics.SubjectMasteryChart.MasteryScores = new List<double>
                {
                    Math.Min(100, Math.Round(exams.Average(e => e.TurkishNet) / 40.0 * 100, 1)),
                    Math.Min(100, Math.Round(exams.Average(e => e.MathNet) / 40.0 * 100, 1)),
                    Math.Min(100, Math.Round(exams.Average(e => e.ScienceNet) / 20.0 * 100, 1)),
                    Math.Min(100, Math.Round(exams.Average(e => e.SocialNet) / 20.0 * 100, 1))
                };
            }

            // 4. Hedef vs Mevcut Durum Grafiği
            analytics.TargetComparisonChart.CurrentAverages = new List<double>
            {
                exams.Any() ? Math.Round(exams.Average(e => e.TurkishNet), 1) : 0,
                exams.Any() ? Math.Round(exams.Average(e => e.MathNet), 1) : 0,
                exams.Any() ? Math.Round(exams.Average(e => e.ScienceNet), 1) : 0,
                exams.Any() ? Math.Round(exams.Average(e => e.SocialNet), 1) : 0
            };
            analytics.TargetComparisonChart.TargetAverages = new List<double> { 35.0, 36.5, 17.5, 16.0 };

            return analytics;
        }
    }
}