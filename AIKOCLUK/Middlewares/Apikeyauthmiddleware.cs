namespace AIKOCLUK.Middlewares
{
    /// <summary>
    /// Projede henüz kullanıcı girişi (Identity/JWT) bulunmadığından,
    /// /api/* altındaki tüm uçları basit bir paylaşılan anahtar (shared secret)
    /// ile korur. İstemci, "X-Api-Key" header'ında appsettings/User Secrets içindeki
    /// ApiSettings:ApiKey değerini göndermek zorundadır.
    /// </summary>
    public class ApiKeyAuthMiddleware
    {
        private const string HeaderName = "X-Api-Key";

        private readonly RequestDelegate _next;
        private readonly string _apiKey;

        public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _apiKey = configuration["ApiSettings:ApiKey"]
                ?? throw new InvalidOperationException("ApiSettings:ApiKey bulunamadı.");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(HeaderName, out var providedKey) ||
                providedKey.ToString() != _apiKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Geçersiz veya eksik API anahtarı." });
                return;
            }

            await _next(context);
        }
    }
}