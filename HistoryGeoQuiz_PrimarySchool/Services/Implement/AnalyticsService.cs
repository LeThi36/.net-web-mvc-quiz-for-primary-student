using HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces;
using HistoryGeoQuiz_PrimarySchool.ViewModels;

namespace HistoryGeoQuiz_PrimarySchool.Services.Implement.Teacher
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ILessonRepository _lessonRepo;
        private readonly ITestResultRepository _testResultRepo;

        public AnalyticsService(ILessonRepository lessonRepo, ITestResultRepository testResultRepo)
        {
            _lessonRepo = lessonRepo;
            _testResultRepo = testResultRepo;
        }

        public async Task<QuestionAnalyticsViewModel?> GetLessonAnalyticsAsync(Guid lessonId, Guid teacherId)
        {
            // Verify lesson ownership
            var lesson = await _lessonRepo.GetByIdForTeacherAsync(lessonId, teacherId);
            
            if (lesson == null) return null;

            // Get all test results for summary metrics
            var results = await _testResultRepo.GetResultsByLessonAsync(lessonId);
            
            // Get all test details for question-specific stats
            var testDetails = await _testResultRepo.GetDetailsByLessonAsync(lessonId);

            var viewModel = new QuestionAnalyticsViewModel
            {
                LessonId = lessonId,
                LessonTitle = lesson.Title,
                TotalParticipants = results.Select(r => r.StudentId).Distinct().Count(),
                AverageScore = results.Any() ? Math.Round(results.Average(r => r.Score), 1) : 0,
                AverageTimeSeconds = results.Any() ? (int)results.Average(r => r.TimeTakenSeconds) : 0,
                PassCount = results.Count(r => r.IsPassed),
                FailCount = results.Count(r => !r.IsPassed),
                TopStudents = results.Take(5).Select(r => new StudentScoreViewModel
                {
                    FullName = r.Student?.FullName ?? "N/A",
                    Score = r.Score,
                    TimeSeconds = r.TimeTakenSeconds
                }).ToList()
            };

            // Group by question
            var questionGroups = testDetails.GroupBy(d => d.QuestionId);

            foreach (var group in questionGroups)
            {
                var question = group.First().Question;
                if (question == null) continue;

                var totalAttempts = group.Count();
                var correctCount = group.Count(d => d.IsCorrect);
                var accuracyRate = totalAttempts > 0 ? (double)correctCount / totalAttempts * 100 : 0;

                viewModel.QuestionStats.Add(new QuestionStatViewModel
                {
                    QuestionId = group.Key,
                    QuestionText = question.QuestionText,
                    TotalAttempts = totalAttempts,
                    CorrectCount = correctCount,
                    IncorrectCount = totalAttempts - correctCount,
                    AccuracyRate = Math.Round(accuracyRate, 1)
                });
            }

            // Order by accuracy rate (hardest first)
            viewModel.QuestionStats = viewModel.QuestionStats.OrderBy(s => s.AccuracyRate).ToList();

            return viewModel;
        }
    }
}
