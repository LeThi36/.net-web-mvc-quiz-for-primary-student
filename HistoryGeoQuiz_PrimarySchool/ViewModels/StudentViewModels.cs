using HistoryGeoQuiz_PrimarySchool.Models;

namespace HistoryGeoQuiz_PrimarySchool.ViewModels
{
    public class StudentDashboardViewModel
    {
        public List<LessonCardViewModel> Lessons { get; set; } = new List<LessonCardViewModel>();
        public List<TestResult> RecentResults { get; set; } = new List<TestResult>();
    }

    public class LessonCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Subject { get; set; } = string.Empty;
        public int LessonNumber { get; set; }
        public int QuestionCount { get; set; }
        public bool HasAttempted { get; set; }
        public double? BestScore { get; set; }
    }

    public class TakeQuizViewModel
    {
        public int LessonId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public List<QuizQuestionViewModel> Questions { get; set; } = new List<QuizQuestionViewModel>();
    }

    public class QuizQuestionViewModel
    {
        public int QuestionId { get; set; }
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<QuizAnswerViewModel> Answers { get; set; } = new List<QuizAnswerViewModel>();
    }

    public class QuizAnswerViewModel
    {
        public int AnswerId { get; set; }
        public string AnswerLabel { get; set; } = string.Empty;
        public string AnswerText { get; set; } = string.Empty;
    }

    public class SubmitQuizViewModel
    {
        public int LessonId { get; set; }
        public Dictionary<int, int> Answers { get; set; } = new Dictionary<int, int>(); // QuestionId -> AnswerId
    }

    public class QuizResultViewModel
    {
        public int TestResultId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double Score { get; set; }
        public bool IsPassed { get; set; }
        public DateTime CompletedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ReviewQuizViewModel
    {
        public int TestResultId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double Score { get; set; }
        public bool IsPassed { get; set; }
        public List<ReviewQuestionViewModel> Questions { get; set; } = new List<ReviewQuestionViewModel>();
    }

    public class ReviewQuestionViewModel
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<ReviewAnswerViewModel> Answers { get; set; } = new List<ReviewAnswerViewModel>();
        public int? SelectedAnswerId { get; set; }
        public int CorrectAnswerId { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class ReviewAnswerViewModel
    {
        public int AnswerId { get; set; }
        public string AnswerLabel { get; set; } = string.Empty;
        public string AnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public bool IsSelected { get; set; }
    }
}
