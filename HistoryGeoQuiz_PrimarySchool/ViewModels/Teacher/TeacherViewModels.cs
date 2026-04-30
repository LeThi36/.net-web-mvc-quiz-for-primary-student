using System.ComponentModel.DataAnnotations;
using HistoryGeoQuiz_PrimarySchool.Models;

namespace HistoryGeoQuiz_PrimarySchool.ViewModels
{
    public class CreateLessonViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên bài học")]
        [Display(Name = "Tên bài học")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn môn học")]
        [Display(Name = "Môn học")]
        public string Subject { get; set; } = string.Empty;

        [Display(Name = "Số thứ tự bài")]
        [Range(1, 100, ErrorMessage = "Số bài phải từ 1-100")]
        public int LessonNumber { get; set; } = 1;

        [Required(ErrorMessage = "Vui lòng chọn lớp học")]
        [Display(Name = "Lớp học")]
        public Guid ClassRoomId { get; set; }

        [Display(Name = "Số câu đúng để đạt xuất sắc")]
        public int? QuestionCountForExcellent { get; set; }

        [Display(Name = "Số câu đúng để đạt giỏi")]
        public int? QuestionCountForGood { get; set; }
    }

    public class CreateQuestionViewModel
    {
        public Guid LessonId { get; set; }
        public string? LessonTitle { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        [Display(Name = "Nội dung câu hỏi")]
        public string QuestionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đáp án A")]
        [Display(Name = "Đáp án A")]
        public string AnswerA { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đáp án B")]
        [Display(Name = "Đáp án B")]
        public string AnswerB { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đáp án C")]
        [Display(Name = "Đáp án C")]
        public string AnswerC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đáp án D")]
        [Display(Name = "Đáp án D")]
        public string AnswerD { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn đáp án đúng")]
        [Display(Name = "Đáp án đúng")]
        public string CorrectAnswer { get; set; } = "A"; // A, B, C, or D
    }

    public class EditQuestionViewModel
    {
        public Guid Id { get; set; }
        public Guid LessonId { get; set; }
        public string? LessonTitle { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        [Display(Name = "Nội dung câu hỏi")]
        public string QuestionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đáp án A")]
        [Display(Name = "Đáp án A")]
        public string AnswerA { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đáp án B")]
        [Display(Name = "Đáp án B")]
        public string AnswerB { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đáp án C")]
        [Display(Name = "Đáp án C")]
        public string AnswerC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đáp án D")]
        [Display(Name = "Đáp án D")]
        public string AnswerD { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn đáp án đúng")]
        [Display(Name = "Đáp án đúng")]
        public string CorrectAnswer { get; set; } = "A";
    }

    public class LessonDetailViewModel
    {
        public Lesson Lesson { get; set; } = null!;
        public List<Question> Questions { get; set; } = new List<Question>();
    }
}
