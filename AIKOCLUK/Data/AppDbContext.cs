using Microsoft.EntityFrameworkCore;
using AIKOCLUK.Models;

namespace AIKOCLUK.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<AiFeedback> AiFeedbacks { get; set; }
        public DbSet<ExamTopicError> ExamTopicErrors { get; set; }

        // --- YENİ EKLENEN YAPAY ZEKA TABLOLARI ---
        public DbSet<AiAdviceHistory> AiAdviceHistories { get; set; }
        public DbSet<AiTargetSubject> AiTargetSubjects { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=yks_coach.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Student - ExamResult (1 - N İlişkisi)
            modelBuilder.Entity<ExamResult>()
                .HasOne(e => e.Student)
                .WithMany(s => s.ExamResults)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student - AiFeedback (1 - N İlişkisi)
            modelBuilder.Entity<AiFeedback>()
                .HasOne(a => a.Student)
                .WithMany(s => s.AiFeedbacks)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ExamResult - ExamTopicError (1 - N İlişkisi)
            modelBuilder.Entity<ExamTopicError>()
                .HasOne(e => e.ExamResult)
                .WithMany(er => er.TopicErrors)
                .HasForeignKey(e => e.ExamResultId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- YENİ EKLENEN İLİŞKİ (AiAdviceHistory - AiTargetSubject) ---
            modelBuilder.Entity<AiTargetSubject>()
                .HasOne<AiAdviceHistory>()
                .WithMany(a => a.HedefKonular)
                .HasForeignKey(t => t.AiAdviceHistoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}