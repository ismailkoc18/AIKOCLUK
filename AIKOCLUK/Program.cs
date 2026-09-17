using AIKOCLUK.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Veritabaný Servis Kaydý (DbContext)
builder.Services.AddDbContext<AppDbContext>();

// --- API Key Kontrolü ---
var apiKey = builder.Configuration["GeminiSettings:ApiKey"];

if (string.IsNullOrEmpty(apiKey))
{
    throw new InvalidOperationException(
        "Gemini API Key bulunamadý! Lütfen 'User Secrets' (secrets.json) konfigürasyonunu kontrol edin."
    );
}

// AI Koç Servisi Kaydý
builder.Services.AddScoped<AIKOCLUK.Services.AiCoachService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();