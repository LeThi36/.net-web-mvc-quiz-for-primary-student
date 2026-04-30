using HistoryGeoQuiz_PrimarySchool.Models;

namespace HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces
{
    /// <summary>
    /// Repository for Lesson entity operations.
    /// </summary>
    public interface ILessonRepository : IRepository<Lesson>
    {
        Task<List<Lesson>> GetByTeacherWithQuestionsAsync(Guid teacherId);
        Task<Lesson?> GetByIdForTeacherAsync(Guid lessonId, Guid teacherId);
        Task<Lesson?> GetWithQuestionsAndAnswersAsync(Guid lessonId, Guid teacherId);
        Task<List<Lesson>> GetActiveByClassAsync(Guid classRoomId);
        Task<Lesson?> GetActiveWithQuestionsAsync(Guid lessonId);
    }
}
