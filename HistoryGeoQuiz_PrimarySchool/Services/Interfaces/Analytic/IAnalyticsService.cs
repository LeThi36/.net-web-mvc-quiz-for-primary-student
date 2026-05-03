using HistoryGeoQuiz_PrimarySchool.ViewModels.Analytic;

namespace HistoryGeoQuiz_PrimarySchool.Services.Interfaces.Analytic
{
    public interface IAnalyticsService
    {
        Task<QuestionAnalyticsViewModel?> GetLessonAnalyticsAsync(Guid lessonId, Guid teacherId);
        Task<StudentReportViewModel?> GetStudentReportAsync(Guid lessonId, Guid studentId, Guid teacherId);
    }
}
