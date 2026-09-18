using AIKOCLUK.Data;
using AIKOCLUK.Middlewares;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;

// --- 1. Serilog Konfigürasyonu ---
// Uygulama loglarýný hem konsola hem de günlük metin dosyalarýna (logs/ klasörüne) yazar
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/aikocluk-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Uygulama baþlatýlýyor...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog'u .NET Host seviyesine baðlýyoruz
    builder.Host.UseSerilog();

    // --- 2. Servislerin Konfigürasyonu ---
    builder.Services.AddControllers();

    // FluentValidation Servis Kayýtlarý
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<AIKOCLUK.Validators.ExamResultCreateDtoValidator>();

    // Klasik Swagger Gen servisini ekliyoruz
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Veritabaný Servis Kaydý (DbContext)
    builder.Services.AddDbContext<AppDbContext>();
    // Repository Servis Kaydý
    builder.Services.AddScoped<AIKOCLUK.Repositories.ICoachRepository, AIKOCLUK.Repositories.CoachRepository>();

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

    // --- 3. HTTP Pipeline Yapýlandýrmasý ---

    // Global Hata Yakalama Middleware'i (Tüm hatalarý merkezi olarak yakalar ve loglar)
    app.UseMiddleware<GlobalExceptionMiddleware>();

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

    // --- 4. Veritabaný Baþlangýç Verisi (Seed Data) ---
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

    // --- 5. Uygulamayý Baþlat ---
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama beklenmeyen bir hata nedeniyle durduruldu!");
}
finally
{
    Log.CloseAndFlush();
}