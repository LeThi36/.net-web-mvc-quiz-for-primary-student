using Microsoft.EntityFrameworkCore;
using HistoryGeoQuiz_PrimarySchool.Data;
using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Repositories.Implement.Core;

namespace HistoryGeoQuiz_PrimarySchool.Repositories.Implement.Teacher
{
    public class QuestionRepository : Repository<Question>, IQuestionRepository
    {
        public QuestionRepository(AppDbContext context) : base(context) { }

        public async Task<Question?> GetWithLessonAndAnswersAsync(Guid questionId)
            => await _dbSet
                .Include(q => q.Lesson)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == questionId);

        public async Task<Question?> GetForTeacherAsync(Guid questionId, Guid teacherId)
            => await _dbSet
                .Include(q => q.Lesson)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == questionId
                    && q.Lesson != null
                    && q.Lesson.CreatedByUserId == teacherId);

        public async Task<int> GetMaxOrderIndexAsync(Guid lessonId)
            => await _dbSet
                .Where(q => q.LessonId == lessonId)
                .MaxAsync(q => (int?)q.OrderIndex) ?? 0;

        public async Task AddAnswersAsync(IEnumerable<Answer> answers)
        {
            await _context.Answers.AddRangeAsync(answers);
        }
    }
}
