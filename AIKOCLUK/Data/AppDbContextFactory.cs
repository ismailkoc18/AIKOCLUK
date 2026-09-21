using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AIKOCLUK.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Konfigürasyon ayarlarını ve User Secrets / appsettings okuma işlemini yapılandırıyoruz
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<AppDbContext>(optional: true)
                .Build();

            var builder = new DbContextOptionsBuilder<AppDbContext>();

            // Not: AppDbContext'inde connection string nasıl tanımlıysa buraya onu yazabilirsin. 
            // Genellikle appsettings.json içindeki bağlantı cümlesi okunur:
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=aikocluk.db";

            builder.UseSqlite(connectionString);

            return new AppDbContext(builder.Options);
        }
    }
}