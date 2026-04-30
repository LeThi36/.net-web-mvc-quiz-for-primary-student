using HistoryGeoQuiz_PrimarySchool.Models;

namespace HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces
{
    /// <summary>
    /// Repository for User entity operations (students, teachers, admins).
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> UsernameExistsAsync(string username);
        Task<List<User>> GetTeachersAsync();
        Task<List<User>> GetStudentsWithoutClassAsync();
        Task<User?> GetStudentWithClassInfoAsync(Guid userId);
        Task<HashSet<string>> GetAllUsernamesAsync();
        void ClearTeacherCache();
    }
}
