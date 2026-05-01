using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.ViewModels;
using HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces;

namespace HistoryGeoQuiz_PrimarySchool.Services.Implement.Teacher
{
    /// <summary>
    /// Lesson service. Contains business logic for lesson management.
    /// All DB queries go through ILessonRepository and ITestResultRepository.
    /// </summary>
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepo;
        private readonly ITestResultRepository _testResultRepo;
        private readonly ILogger<LessonService> _logger;

        public LessonService(
            ILessonRepository lessonRepo,
            ITestResultRepository testResultRepo,
            ILogger<LessonService> logger)
        {
            _lessonRepo = lessonRepo;
            _testResultRepo = testResultRepo;
            _logger = logger;
        }

        public async Task<List<Lesson>> GetLessonsByTeacherAsync(Guid teacherId)
            => await _lessonRepo.GetByTeacherWithQuestionsAsync(teacherId);

        public async Task<Lesson?> GetLessonByIdAsync(Guid lessonId, Guid teacherId)
            => await _lessonRepo.GetByIdForTeacherAsync(lessonId, teacherId);

        public async Task<Lesson> CreateLessonAsync(CreateLessonViewModel model, Guid teacherId)
        {
            var lesson = new Lesson
            {
                Title = model.Title,
                Description = model.Description,
                Subject = model.Subject,
                LessonNumber = model.LessonNumber,
                CreatedByUserId = teacherId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ClassRoomId = model.ClassRoomId,
                QuestionCountForExcellent = model.QuestionCountForExcellent,
                QuestionCountForGood = model.QuestionCountForGood,
                TimeLimitMinutes = model.TimeLimitMinutes
            };

            await _lessonRepo.AddAsync(lesson);
            await _lessonRepo.SaveChangesAsync();

            _logger.LogInformation("Lesson created: ID={LessonId}, Subject={Subject}, Teacher={TeacherId}",
                lesson.Id, lesson.Subject, teacherId);
            return lesson;
        }

        public async Task<bool> UpdateLessonAsync(Guid lessonId, CreateLessonViewModel model, Guid teacherId)
        {
            var lesson = await _lessonRepo.GetByIdForTeacherAsync(lessonId, teacherId);
            if (lesson == null) return false;

            lesson.Title = model.Title;
            lesson.Description = model.Description;
            lesson.Subject = model.Subject;
            lesson.LessonNumber = model.LessonNumber;
            lesson.ClassRoomId = model.ClassRoomId;
            lesson.QuestionCountForExcellent = model.QuestionCountForExcellent;
            lesson.QuestionCountForGood = model.QuestionCountForGood;
            lesson.TimeLimitMinutes = model.TimeLimitMinutes;

            await _lessonRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLessonAsync(Guid lessonId, Guid teacherId)
        {
            var lesson = await _lessonRepo.GetByIdForTeacherAsync(lessonId, teacherId);
            if (lesson == null)
                return false;

            // Soft delete: mark as deleted, data is preserved
            lesson.IsDeleted = true;
            lesson.DeletedAt = DateTime.UtcNow;
            lesson.IsActive = false;

            await _lessonRepo.SaveChangesAsync();

            _logger.LogInformation("Lesson soft-deleted: ID={LessonId}, Teacher={TeacherId}", lessonId, teacherId);
            return true;
        }

        public async Task<Lesson?> GetLessonDetailWithQuestionsAsync(Guid lessonId, Guid teacherId)
            => await _lessonRepo.GetWithQuestionsAndAnswersAsync(lessonId, teacherId);

        public async Task<bool> UpdateLessonConfigAsync(Guid lessonId, int? timeLimit, int? excellent, int? good, Guid teacherId)
        {
            var lesson = await _lessonRepo.GetByIdForTeacherAsync(lessonId, teacherId);
            if (lesson == null) return false;

            lesson.TimeLimitMinutes = timeLimit;
            lesson.QuestionCountForExcellent = excellent;
            lesson.QuestionCountForGood = good;

            await _lessonRepo.SaveChangesAsync();
            return true;
        }
    }
}

