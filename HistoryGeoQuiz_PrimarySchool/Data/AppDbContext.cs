using Microsoft.EntityFrameworkCore;
using HistoryGeoQuiz_PrimarySchool.Models;

namespace HistoryGeoQuiz_PrimarySchool.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<TestDetail> TestDetails { get; set; }

        public DbSet<ClassRoom> ClassRooms { get; set; }
        public DbSet<TeacherAssignment> TeacherAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasConversion<string>();

                // Student Key to Class
                entity.HasOne(e => e.ClassRoom)
                    .WithMany(c => c.Students)
                    .HasForeignKey(e => e.ClassRoomId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ClassRoom configuration
            modelBuilder.Entity<ClassRoom>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClassName).IsRequired().HasMaxLength(50);
                
                // Homeroom Teacher
                entity.HasOne(e => e.HomeroomTeacher)
                    .WithMany() // Assuming one teacher can be homeroom for multiple classes or we don't track back strictly
                    .HasForeignKey(e => e.HomeroomTeacherId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // TeacherAssignment configuration
            modelBuilder.Entity<TeacherAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Subject).IsRequired();

                entity.HasOne(e => e.Teacher)
                    .WithMany(t => t.TeacherAssignments)
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ClassRoom)
                    .WithMany(c => c.TeacherAssignments)
                    .HasForeignKey(e => e.ClassRoomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Lesson configuration
            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(50);
                
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.CreatedLessons)
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ClassRoom)
                    .WithMany(c => c.Lessons)
                    .HasForeignKey(e => e.ClassRoomId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Question configuration
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuestionText).IsRequired();
                
                entity.HasOne(e => e.Lesson)
                    .WithMany(l => l.Questions)
                    .HasForeignKey(e => e.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Answer configuration
            modelBuilder.Entity<Answer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AnswerText).IsRequired();
                entity.Property(e => e.AnswerLabel).IsRequired().HasMaxLength(5);
                
                entity.HasOne(e => e.Question)
                    .WithMany(q => q.Answers)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TestResult configuration
            modelBuilder.Entity<TestResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Student)
                    .WithMany(u => u.TestResults)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Lesson)
                    .WithMany(l => l.TestResults)
                    .HasForeignKey(e => e.LessonId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TestDetail configuration
            modelBuilder.Entity<TestDetail>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.TestResult)
                    .WithMany(tr => tr.Details)
                    .HasForeignKey(e => e.TestResultId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Question)
                    .WithMany(q => q.TestDetails)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.SelectedAnswer)
                    .WithMany(a => a.TestDetails)
                    .HasForeignKey(e => e.SelectedAnswerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================
            // Global Query Filters — Soft Delete
            // All queries automatically filter out deleted records.
            // Use .IgnoreQueryFilters() when you need to include deleted records.
            // ============================================
            modelBuilder.Entity<Lesson>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Question>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Answer>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ClassRoom>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<TestResult>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<TeacherAssignment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<TestDetail>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
