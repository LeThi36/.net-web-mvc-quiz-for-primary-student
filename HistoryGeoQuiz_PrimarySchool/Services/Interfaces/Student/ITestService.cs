using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.ViewModels;

namespace HistoryGeoQuiz_PrimarySchool.Services.Interfaces
{
    public interface ITestService
    {
        Task<StudentDashboardViewModel> GetStudentDashboardAsync(Guid userId);
        Task<(User? Student, string? ClassName, string? HomeroomTeacher)> GetStudentInfoAsync(Guid userId);
        Task<TakeQuizViewModel?> GetQuizAsync(Guid lessonId, Guid userId);
        Task<TestResult> SubmitQuizAsync(Guid lessonId, Dictionary<string, Guid> answers, Guid userId, int timeTakenSeconds = 0);
        Task<QuizResultViewModel?> GetResultAsync(Guid testResultId, Guid userId);
        Task<ReviewQuizViewModel?> GetReviewAsync(Guid testResultId, Guid userId);
        Task<List<TestResult>> GetHistoryAsync(Guid userId);
    }
}
