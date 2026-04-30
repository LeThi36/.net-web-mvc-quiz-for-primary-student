using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.ViewModels;

namespace HistoryGeoQuiz_PrimarySchool.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<Question?> GetQuestionForEditAsync(Guid questionId, Guid teacherId);
        Task<Question> CreateQuestionAsync(CreateQuestionViewModel model, Guid teacherId);
        Task<bool> UpdateQuestionAsync(EditQuestionViewModel model, Guid teacherId);
        Task<bool> DeleteQuestionAsync(Guid questionId, Guid teacherId);
        Task<int> ImportQuestionsFromExcelAsync(Guid lessonId, IFormFile file, Guid teacherId);
    }
}
