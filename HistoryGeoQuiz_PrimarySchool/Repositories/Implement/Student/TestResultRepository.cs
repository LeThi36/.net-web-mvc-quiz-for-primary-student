using Microsoft.EntityFrameworkCore;
using HistoryGeoQuiz_PrimarySchool.Data;
using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Repositories.Implement.Core;

namespace HistoryGeoQuiz_PrimarySchool.Repositories.Implement.Student
{
    public class TestResultRepository : Repository<TestResult>, ITestResultRepository
    {
        public TestResultRepository(AppDbContext context) : base(context) { }

        // P2: AsNoTracking for read-only queries
        public async Task<List<TestResult>> GetByStudentAsync(Guid studentId)
            => await _dbSet
                .AsNoTracking()
                .Where(tr => tr.StudentId == studentId)
                .ToListAsync();

        // P2: AsNoTracking for read-only queries
        public async Task<List<TestResult>> GetRecentByStudentAsync(Guid studentId, int count)
            => await _dbSet
                .AsNoTracking()
                .Where(tr => tr.StudentId == studentId)
                .Include(tr => tr.Lesson)
                .OrderByDescending(tr => tr.CompletedAt)
                .Take(count)
                .ToListAsync();

        // P2: AsNoTracking for read-only result view
        public async Task<TestResult?> GetWithLessonAsync(Guid testResultId, Guid studentId)
            => await _dbSet
                .AsNoTracking()
                .Include(tr => tr.Lesson)
                .FirstOrDefaultAsync(tr => tr.Id == testResultId && tr.StudentId == studentId);

        // P2: AsNoTracking for read-only review
        public async Task<TestResult?> GetWithFullReviewAsync(Guid testResultId, Guid studentId)
            => await _dbSet
                .AsNoTracking()
                .Include(tr => tr.Lesson)
                .Include(tr => tr.Details)
                    .ThenInclude(d => d.Question)
                        .ThenInclude(q => q!.Answers)
                .FirstOrDefaultAsync(tr => tr.Id == testResultId && tr.StudentId == studentId);

        public async Task AddDetailsAsync(IEnumerable<TestDetail> details)
        {
            await _context.TestDetails.AddRangeAsync(details);
        }

        public async Task<List<TestDetail>> GetDetailsByLessonAsync(Guid lessonId)
        {
            return await _context.TestDetails
                .AsNoTracking()
                .Include(d => d.Question)
                .Where(d => d.TestResult!.LessonId == lessonId)
                .ToListAsync();
        }

        public async Task<int> GetTotalParticipantsByLessonAsync(Guid lessonId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(r => r.LessonId == lessonId)
                .Select(r => r.StudentId)
                .Distinct()
                .CountAsync();
        }

        public async Task<List<TestResult>> GetResultsByLessonAsync(Guid lessonId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(r => r.Student)
                .Where(r => r.LessonId == lessonId)
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.TimeTakenSeconds)
                .ToListAsync();
        }
    }
}
