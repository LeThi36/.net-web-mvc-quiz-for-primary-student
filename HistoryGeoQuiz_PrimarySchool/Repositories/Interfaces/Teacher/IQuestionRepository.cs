using HistoryGeoQuiz_PrimarySchool.Models;

namespace HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces
{
    /// <summary>
    /// Repository for Question and Answer entity operations.
    /// </summary>
    public interface IQuestionRepository : IRepository<Question>
    {
        Task<Question?> GetWithLessonAndAnswersAsync(Guid questionId);
        Task<Question?> GetForTeacherAsync(Guid questionId, Guid teacherId);
        Task<int> GetMaxOrderIndexAsync(Guid lessonId);
        Task AddAnswersAsync(IEnumerable<Answer> answers);
    }
}
