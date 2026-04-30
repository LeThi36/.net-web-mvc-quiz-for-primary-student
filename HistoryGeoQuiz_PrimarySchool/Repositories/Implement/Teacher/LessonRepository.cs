using Microsoft.EntityFrameworkCore;
using HistoryGeoQuiz_PrimarySchool.Data;
using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Repositories.Implement.Core;

namespace HistoryGeoQuiz_PrimarySchool.Repositories.Implement.Teacher
{
    public class LessonRepository : Repository<Lesson>, ILessonRepository
    {
        public LessonRepository(AppDbContext context) : base(context) { }

        // P2: AsNoTracking for read-only dashboard queries
        public async Task<List<Lesson>> GetByTeacherWithQuestionsAsync(Guid teacherId)
            => await _dbSet
                .AsNoTracking()
                .Where(l => l.CreatedByUserId == teacherId)
                .Include(l => l.Questions)
                .Include(l => l.ClassRoom)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

        public async Task<Lesson?> GetByIdForTeacherAsync(Guid lessonId, Guid teacherId)
            => await _dbSet
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.CreatedByUserId == teacherId);

        public async Task<Lesson?> GetWithQuestionsAndAnswersAsync(Guid lessonId, Guid teacherId)
            => await _dbSet
                .Include(l => l.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.CreatedByUserId == teacherId);

        // P2: AsNoTracking for student dashboard (read-only)
        public async Task<List<Lesson>> GetActiveByClassAsync(Guid classRoomId)
            => await _dbSet
                .AsNoTracking()
                .Where(l => l.IsActive && l.ClassRoomId == classRoomId)
                .Include(l => l.Questions)
                .Include(l => l.CreatedByUser)
                .OrderBy(l => l.Subject)
                .ThenBy(l => l.LessonNumber)
                .ToListAsync();

        public async Task<Lesson?> GetActiveWithQuestionsAsync(Guid lessonId)
            => await _dbSet
                .Include(l => l.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.IsActive);
    }
}
