namespace HistoryGeoQuiz_PrimarySchool.Enums
{
    public enum UserRole
    {
        Student,
        Teacher,
        Admin
    }

    public static class UserRoleExtensions
    {
        public static readonly UserRole[] AllowedRegistrationRoles = { UserRole.Student };
    }
}
