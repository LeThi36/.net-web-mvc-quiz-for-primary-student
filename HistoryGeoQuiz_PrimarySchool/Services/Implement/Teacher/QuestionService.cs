using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.ViewModels;
using HistoryGeoQuiz_PrimarySchool.ViewModels.Teacher;
using HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Helpers;
using ExcelDataReader;
using System.Web;

namespace HistoryGeoQuiz_PrimarySchool.Services.Implement.Teacher
{
    /// <summary>
    /// Question service. Contains business logic for question management.
    /// All DB queries go through IQuestionRepository and ILessonRepository.
    /// </summary>
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepo;
        private readonly ILessonRepository _lessonRepo;
        private readonly ILogger<QuestionService> _logger;

        public QuestionService(
            IQuestionRepository questionRepo,
            ILessonRepository lessonRepo,
            ILogger<QuestionService> logger)
        {
            _questionRepo = questionRepo;
            _lessonRepo = lessonRepo;
            _logger = logger;
        }

        public async Task<Question?> GetQuestionForEditAsync(Guid questionId, Guid teacherId)
            => await _questionRepo.GetForTeacherAsync(questionId, teacherId);

        public async Task<Question> CreateQuestionAsync(CreateQuestionViewModel model, Guid teacherId)
        {
            // Business rule: verify lesson ownership
            var lesson = await _lessonRepo.GetByIdForTeacherAsync(model.LessonId, teacherId);
            if (lesson == null)
                throw new UnauthorizedAccessException("Lesson not found or access denied");

            var maxOrder = await _questionRepo.GetMaxOrderIndexAsync(model.LessonId);

            var question = new Question
            {
                LessonId = model.LessonId,
                QuestionText = model.QuestionText,
                OrderIndex = maxOrder + 1,
                CreatedAt = DateTimeHelper.GetVietnamTime()
            };

            await _questionRepo.AddAsync(question);
            await _questionRepo.SaveChangesAsync();

            // Build answers
            var answers = new List<Answer>
            {
                new Answer { QuestionId = question.Id, AnswerText = model.AnswerA, AnswerLabel = "A", IsCorrect = model.CorrectAnswer == "A" },
                new Answer { QuestionId = question.Id, AnswerText = model.AnswerB, AnswerLabel = "B", IsCorrect = model.CorrectAnswer == "B" },
                new Answer { QuestionId = question.Id, AnswerText = model.AnswerC, AnswerLabel = "C", IsCorrect = model.CorrectAnswer == "C" },
                new Answer { QuestionId = question.Id, AnswerText = model.AnswerD, AnswerLabel = "D", IsCorrect = model.CorrectAnswer == "D" }
            };

            await _questionRepo.AddAnswersAsync(answers);
            await _questionRepo.SaveChangesAsync();

            return question;
        }

        public async Task<bool> UpdateQuestionAsync(EditQuestionViewModel model, Guid teacherId)
        {
            var question = await _questionRepo.GetForTeacherAsync(model.Id, teacherId);
            if (question == null) return false;

            question.QuestionText = model.QuestionText;

            foreach (var answer in question.Answers)
            {
                switch (answer.AnswerLabel)
                {
                    case "A": answer.AnswerText = model.AnswerA; answer.IsCorrect = model.CorrectAnswer == "A"; break;
                    case "B": answer.AnswerText = model.AnswerB; answer.IsCorrect = model.CorrectAnswer == "B"; break;
                    case "C": answer.AnswerText = model.AnswerC; answer.IsCorrect = model.CorrectAnswer == "C"; break;
                    case "D": answer.AnswerText = model.AnswerD; answer.IsCorrect = model.CorrectAnswer == "D"; break;
                }
            }

            await _questionRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuestionAsync(Guid questionId, Guid teacherId)
        {
            var question = await _questionRepo.GetForTeacherAsync(questionId, teacherId);
            if (question == null) return false;

            // Soft delete: mark as deleted, preserves TestDetail references
            question.IsDeleted = true;
            question.DeletedAt = DateTimeHelper.GetVietnamTime();
            await _questionRepo.SaveChangesAsync();
            return true;
        }

        public async Task<int> ImportQuestionsFromExcelAsync(Guid lessonId, IFormFile file, Guid teacherId)
        {
            // Business rule: verify lesson ownership
            var lesson = await _lessonRepo.GetByIdForTeacherAsync(lessonId, teacherId);
            if (lesson == null)
                throw new UnauthorizedAccessException("Lesson not found or access denied");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var maxOrder = await _questionRepo.GetMaxOrderIndexAsync(lessonId);

            using var stream = file.OpenReadStream();
            using var excelReader = ExcelReaderFactory.CreateReader(stream);

            var dataSet = excelReader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
            });

            int count = 0;

            if (dataSet.Tables.Count > 0)
            {
                var table = dataSet.Tables[0];

                // Phase 1: Parse all questions from Excel into memory
                var questionsData = new List<(Question Question, string AnsA, string AnsB, string AnsC, string AnsD, string Correct)>();

                foreach (System.Data.DataRow row in table.Rows)
                {
                    string GetCell(params string[] possibleNames)
                    {
                        foreach (var name in possibleNames)
                        {
                            if (table.Columns.Contains(name))
                                return row[name]?.ToString() ?? "";
                        }
                        return "";
                    }

                    string qText = GetCell("NoiDungCauHoi", "QuestionText");
                    if (string.IsNullOrWhiteSpace(qText)) continue;

                    // Business rule: sanitize input from Excel to prevent XSS
                    qText = HttpUtility.HtmlEncode(qText);

                    var question = new Question
                    {
                        LessonId = lessonId,
                        QuestionText = qText,
                        OrderIndex = maxOrder + (++count),
                        CreatedAt = DateTimeHelper.GetVietnamTime()
                    };

                    string ansA = HttpUtility.HtmlEncode(GetCell("DapAnA", "AnswerA"));
                    string ansB = HttpUtility.HtmlEncode(GetCell("DapAnB", "AnswerB"));
                    string ansC = HttpUtility.HtmlEncode(GetCell("DapAnC", "AnswerC"));
                    string ansD = HttpUtility.HtmlEncode(GetCell("DapAnD", "AnswerD"));
                    string correct = GetCell("DapAnDung", "CorrectAnswer").Trim().ToUpper();

                    questionsData.Add((question, ansA, ansB, ansC, ansD, correct));
                }

                // Phase 2: Batch insert all questions (1 round-trip)
                var questions = questionsData.Select(d => d.Question).ToList();
                await _questionRepo.AddRangeAsync(questions);
                await _questionRepo.SaveChangesAsync(); // Question.Id values are now populated

                // Phase 3: Build all answers using the generated Question IDs, then batch insert (1 round-trip)
                var allAnswers = new List<Answer>();
                foreach (var data in questionsData)
                {
                    allAnswers.Add(new Answer { QuestionId = data.Question.Id, AnswerText = data.AnsA, AnswerLabel = "A", IsCorrect = data.Correct == "A" });
                    allAnswers.Add(new Answer { QuestionId = data.Question.Id, AnswerText = data.AnsB, AnswerLabel = "B", IsCorrect = data.Correct == "B" });
                    allAnswers.Add(new Answer { QuestionId = data.Question.Id, AnswerText = data.AnsC, AnswerLabel = "C", IsCorrect = data.Correct == "C" });
                    allAnswers.Add(new Answer { QuestionId = data.Question.Id, AnswerText = data.AnsD, AnswerLabel = "D", IsCorrect = data.Correct == "D" });
                }

                await _questionRepo.AddAnswersAsync(allAnswers);
                await _questionRepo.SaveChangesAsync();
            }

            _logger.LogInformation("Imported {Count} questions for lesson {LessonId}", count, lessonId);
            return count;
        }
    }
}
