using AIKOCLUK.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Servislerin Konfigürasyonu
builder.Services.AddControllers();

// Klasik Swagger Gen servisini ekliyoruz
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Veritabaný Servis Kaydý (DbContext)
builder.Services.AddDbContext<AppDbContext>();

// --- API Key Kontrolü ---
var apiKey = builder.Configuration["Gemini:ApiKey"];
if (string.IsNullOrEmpty(apiKey))
{
    throw new InvalidOperationException(
        "Gemini API Key bulunamadý! Lütfen 'User Secrets' (secrets.json) konfigürasyonunu kontrol edin."
    );
}

// AI Koç Servisi ve HttpClient Kaydý
builder.Services.AddHttpClient<AIKOCLUK.Services.AiCoachService>();

var app = builder.Build();

// 2. HTTP Pipeline Yapýlandýrmasý
if (app.Environment.IsDevelopment())
{
    // Klasik Swagger middleware'lerini aktif ediyoruz
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AIKOCLUK v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 3. Veritabaný Baþlangýç Verisi (Seed Data)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Veritabanýnýn ve tablolarýn hazýr olduðundan emin oluyoruz
    context.Database.EnsureCreated();

    // Veritabaný boþsa test öðrencisi ve deneme verisi ekliyoruz
    if (!context.Students.Any())
    {
        var testStudent = new AIKOCLUK.Models.Student
        {
            Name = "Ahmet Yýlmaz",
            TargetUniversity = "ÝSTANBUL TEKNÝK ÜNÝVERSÝTESÝ",
            TargetDepartment = "Bilgisayar Mühendisliði",
            Field = "SAY",
            LearningStyle = "Görsel ve Pratik Aðýrlýklý",
            DailyAvailableStudyHours = 6,
            CurrentStressLevel = 8,
            WeakSubjects = "Geometri, Fizik (Aydýnlanma ve Optik)"
        };

        context.Students.Add(testStudent);
        context.SaveChanges();

        var testExam = new AIKOCLUK.Models.ExamResult
        {
            StudentId = testStudent.Id,
            ExamType = "TYT",
            ExamDate = DateTime.UtcNow.AddDays(-3),
            TurkishNet = 32.5,
            MathNet = 24.0,
            ScienceNet = 11.5,
            SocialNet = 15.0,
            TimeManagementIssue = true
        };

        context.ExamResults.Add(testExam);
        context.SaveChanges();
    }
}

// 4. Uygulamayý Baþlat
app.Run();