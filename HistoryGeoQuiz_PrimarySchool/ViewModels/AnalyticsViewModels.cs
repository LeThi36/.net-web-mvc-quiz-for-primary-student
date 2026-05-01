using System;
using System.Collections.Generic;
using HistoryGeoQuiz_PrimarySchool.Models;

namespace HistoryGeoQuiz_PrimarySchool.ViewModels
{
    public class QuestionAnalyticsViewModel
    {
        public Guid LessonId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public int TotalParticipants { get; set; }
        public double AverageScore { get; set; }
        public int AverageTimeSeconds { get; set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public List<StudentScoreViewModel> TopStudents { get; set; } = new List<StudentScoreViewModel>();
        public List<QuestionStatViewModel> QuestionStats { get; set; } = new List<QuestionStatViewModel>();
    }

    public class StudentScoreViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public double Score { get; set; }
        public int TimeSeconds { get; set; }
    }

    public class QuestionStatViewModel
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public int CorrectCount { get; set; }
        public int IncorrectCount { get; set; }
        public double AccuracyRate { get; set; } // Percentage of correct answers
        public double ErrorRate => 100 - AccuracyRate;
        
        // To highlight particularly difficult questions
        public bool IsHardQuestion => AccuracyRate < 50; 
    }
}
