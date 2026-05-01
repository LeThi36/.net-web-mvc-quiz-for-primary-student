using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using HistoryGeoQuiz_PrimarySchool.Data;
using HistoryGeoQuiz_PrimarySchool.Models;
using HistoryGeoQuiz_PrimarySchool.Enums;
using HistoryGeoQuiz_PrimarySchool.Repositories.Interfaces;
using HistoryGeoQuiz_PrimarySchool.Repositories.Implement.Core;

namespace HistoryGeoQuiz_PrimarySchool.Repositories.Implement.Auth
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly IMemoryCache _cache;
        private const string TeachersCacheKey = "teachers_list";

        public UserRepository(AppDbContext context, IMemoryCache cache) : base(context)
        {
            _cache = cache;
        }

        public async Task<User?> GetByUsernameAsync(string username)
            => await _dbSet
                .Include(u => u.Avatar)
                .FirstOrDefaultAsync(u => u.Username == username);

        public async Task<bool> UsernameExistsAsync(string username)
            => await _dbSet.AnyAsync(u => u.Username == username);

        // P5: Cached for 10 minutes (dropdown data that rarely changes)
        public async Task<List<User>> GetTeachersAsync()
        {
            var teachers = await _cache.GetOrCreateAsync(TeachersCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _dbSet
                    .AsNoTracking()
                    .Where(u => u.Role == UserRole.Teacher)
                    .ToListAsync();
            });
            return teachers ?? new List<User>();
        }

        // P2: AsNoTracking for read-only query
        public async Task<List<User>> GetStudentsWithoutClassAsync()
            => await _dbSet
                .AsNoTracking()
                .Where(u => u.Role == UserRole.Student && u.ClassRoomId == null)
                .OrderBy(u => u.FullName)
                .ToListAsync();

        // P2: Tracking enabled for student info to support navigation property fix-up
        public async Task<User?> GetStudentWithClassInfoAsync(Guid userId)
            => await _dbSet
                .Include(u => u.Avatar)
                .Include(u => u.ClassRoom)
                    .ThenInclude(c => c!.HomeroomTeacher)
                .FirstOrDefaultAsync(u => u.Id == userId);

        public void ClearTeacherCache()
        {
            _cache.Remove(TeachersCacheKey);
        }

        public async Task<HashSet<string>> GetAllUsernamesAsync()
        {
            return (await _dbSet
                .AsNoTracking()
                .Select(u => u.Username.ToLower())
                .ToListAsync())
                .ToHashSet();
        }
    }
}
