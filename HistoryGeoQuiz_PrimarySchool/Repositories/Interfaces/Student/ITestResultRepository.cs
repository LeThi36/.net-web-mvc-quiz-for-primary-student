using HistoryGeoQuiz_PrimarySchool.Models;

namespace HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces
{
    /// <summary>
    /// Repository for TestResult and TestDetail entity operations.
    /// </summary>
    public interface ITestResultRepository : IRepository<TestResult>
    {
        Task<List<TestResult>> GetByStudentAsync(Guid studentId);
        Task<List<TestResult>> GetRecentByStudentAsync(Guid studentId, int count);
        Task<TestResult?> GetWithLessonAsync(Guid testResultId, Guid studentId);
        Task<TestResult?> GetWithFullReviewAsync(Guid testResultId, Guid studentId);
        Task AddDetailsAsync(IEnumerable<TestDetail> details);
    }
}
