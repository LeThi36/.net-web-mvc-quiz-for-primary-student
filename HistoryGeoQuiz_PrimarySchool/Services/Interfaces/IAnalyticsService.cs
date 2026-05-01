using HistoryGeoQuiz_PrimarySchool.ViewModels;

namespace HistoryGeoQuiz_PrimarySchool.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<QuestionAnalyticsViewModel?> GetLessonAnalyticsAsync(Guid lessonId, Guid teacherId);
    }
}
