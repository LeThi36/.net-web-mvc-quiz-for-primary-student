using HistoryGeoQuiz_PrimarySchool.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HistoryGeoQuiz_PrimarySchool.Helpers;
using HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.Enums;

namespace HistoryGeoQuiz_PrimarySchool.Services.Implement.Auth
{
    /// <summary>
    /// Authentication service. Contains ONLY business logic.
    /// All DB queries go through IUserRepository.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepo, ILogger<AuthService> logger, IConfiguration config)
        {
            _userRepo = userRepo;
            _logger = logger;
            _config = config;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            var user = await _userRepo.GetByUsernameAsync(username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                _logger.LogWarning("Failed login attempt for username: {Username}", username);
                return null;
            }

            _logger.LogInformation("User {Username} logged in with role {Role}", username, user.Role);
            return user;
        }

        public async Task<(bool Success, string? Error)> RegisterAsync(
            string username, string password, string fullName, UserRole role, Gender gender)
        {
            // Business rule: validate role
            if (!UserRoleExtensions.AllowedRegistrationRoles.Contains(role))
                return (false, "Vai trò không hợp lệ! Chỉ được chọn Học sinh.");

            // Business rule: unique username
            if (await _userRepo.UsernameExistsAsync(username))
                return (false, "Tên đăng nhập đã tồn tại!");

            // Business logic: hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Username = username,
                Password = hashedPassword,
                FullName = fullName,
                Role = role,
                Gender = gender,
                CreatedAt = DateTimeHelper.GetVietnamTime()
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            _logger.LogInformation("New user registered: {Username} with role {Role}", username, role);
            return (true, null);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _userRepo.UsernameExistsAsync(username);
        }

        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var avatarUrl = user.Avatar?.Url ?? AvatarHelper.ForUser(user.Id.ToString(), user.Gender);

            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(ClaimTypes.Name, user.Username),
                new System.Security.Claims.Claim(ClaimTypes.Role, user.Role.ToString()),
                new System.Security.Claims.Claim("FullName", user.FullName),
                new System.Security.Claims.Claim("AvatarUrl", avatarUrl)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
