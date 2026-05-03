using ClosedXML.Excel;
using HistoryGeoQuiz_PrimarySchool.Enums;
using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Services.Interfaces;
using HistoryGeoQuiz_PrimarySchool.ViewModels;
using HistoryGeoQuiz_PrimarySchool.ViewModels.Admin;
using HistoryGeoQuiz_PrimarySchool.Helpers;
using Microsoft.AspNetCore.Http;
using System.Web;

namespace HistoryGeoQuiz_PrimarySchool.Services.Implement.Admin
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepo, ILogger<UserService> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<List<User>> GetTeachersAsync()
        {
            return await _userRepo.GetTeachersAsync();
        }

        public async Task<ImportTeachersResultViewModel> ImportTeachersFromExcelAsync(IFormFile file)
        {
            var result = new ImportTeachersResultViewModel();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // Skip header row

            // Load all existing usernames to check for duplicates (Optimized)
            var existingUsernames = await _userRepo.GetAllUsernamesAsync();
            
            var newTeachers = new List<User>();

            foreach (var row in rows)
            {
                result.TotalRows++;
                var rowNum = row.RowNumber();

                var fullName = row.Cell(1).GetString()?.Trim();
                var username = row.Cell(2).GetString()?.Trim();
                var password = row.Cell(3).GetString()?.Trim();
                var genderStr = row.Cell(4).GetString()?.Trim()?.ToLower();
                
                Gender gender = Gender.Male;
                if (genderStr == "nữ" || genderStr == "female" || genderStr == "nu") 
                    gender = Gender.Female;

                // Validate row data
                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    result.Errors.Add(new ImportTeacherError
                    {
                        RowNumber = rowNum,
                        FullName = fullName,
                        Username = username,
                        ErrorMessage = "Thiếu thông tin (Họ tên, Tên đăng nhập hoặc Mật khẩu)"
                    });
                    result.ErrorCount++;
                    continue;
                }

                // Check if username already exists (in-memory lookup)
                if (existingUsernames.Contains(username.ToLower()))
                {
                    result.Errors.Add(new ImportTeacherError
                    {
                        RowNumber = rowNum,
                        FullName = fullName,
                        Username = username,
                        ErrorMessage = "Tên đăng nhập đã tồn tại"
                    });
                    result.ErrorCount++;
                    continue;
                }

                // Track new username to detect duplicates within the same import file
                existingUsernames.Add(username.ToLower());

                // Create teacher account
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                var teacher = new User
                {
                    FullName = fullName,
                    Username = username, // Username kept as-is for login matching
                    Password = hashedPassword,
                    Gender = gender,
                    Role = UserRole.Teacher,
                    CreatedAt = DateTimeHelper.GetVietnamTime()
                };

                newTeachers.Add(teacher);
                result.SuccessfulTeachers.Add(new ImportTeacherSuccess
                {
                    FullName = fullName,
                    Username = username
                });
                result.SuccessCount++;
            }

            if (newTeachers.Any())
            {
                await _userRepo.AddRangeAsync(newTeachers);
                await _userRepo.SaveChangesAsync();
                _userRepo.ClearTeacherCache();
            }

            return result;
        }

        public async Task<(bool Success, string? Error)> CreateTeacherAsync(CreateTeacherViewModel model)
        {
            if (await _userRepo.UsernameExistsAsync(model.Username))
            {
                return (false, "Tên đăng nhập đã tồn tại trong hệ thống.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var teacher = new User
            {
                FullName = model.FullName,
                Username = model.Username,
                Password = hashedPassword,
                Gender = model.Gender,
                Role = UserRole.Teacher,
                CreatedAt = DateTimeHelper.GetVietnamTime()
            };

            await _userRepo.AddAsync(teacher);
            await _userRepo.SaveChangesAsync();
            _userRepo.ClearTeacherCache();
            return (true, null);
        }
    }
}
