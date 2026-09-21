using System.Net;
using System.Text.Json;

namespace AIKOCLUK.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Hatayı Serilog aracılığıyla dosyaya ve konsola logluyoruz
                _logger.LogError(ex, "İşlenmeyen Hata: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                statusCode = context.Response.StatusCode,
                message = "Sunucu tarafında beklenmeyen bir hata oluştu.",
                // Hata detayları sadece Development ortamında dönülür; production'da
                // iç implementasyon/altyapı bilgisi dışarı sızmaz.
                detailedError = _environment.IsDevelopment() ? exception.Message : null
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}