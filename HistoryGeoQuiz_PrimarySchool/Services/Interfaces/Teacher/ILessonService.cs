using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.ViewModels;

namespace HistoryGeoQuiz_PrimarySchool.Services.Interfaces
{
    public interface ILessonService
    {
        Task<List<Lesson>> GetLessonsByTeacherAsync(Guid teacherId);
        Task<Lesson?> GetLessonByIdAsync(Guid lessonId, Guid teacherId);
        Task<Lesson> CreateLessonAsync(CreateLessonViewModel model, Guid teacherId);
        Task<bool> UpdateLessonAsync(Guid lessonId, CreateLessonViewModel model, Guid teacherId);
        Task<bool> DeleteLessonAsync(Guid lessonId, Guid teacherId);
        Task<Lesson?> GetLessonDetailWithQuestionsAsync(Guid lessonId, Guid teacherId);
        Task<bool> UpdateLessonConfigAsync(Guid lessonId, int? timeLimit, int? excellent, int? good, Guid teacherId);
    }
}
