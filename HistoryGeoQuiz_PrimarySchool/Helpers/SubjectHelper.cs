namespace HistoryGeoQuiz_PrimarySchool.Helpers
{
    public static class SubjectHelper
    {
        public static readonly Dictionary<string, (string Name, string Icon, string Color)> Subjects = new()
        {
            { "Toan", ("Toán", "🔢", "primary") },
            { "TiengViet", ("Tiếng Việt", "📚", "danger") },
            { "TiengAnh", ("Tiếng Anh", "🌎", "info") },
            { "TNXH", ("TN & XH", "🌿", "success") },
            { "LichSu", ("Lịch Sử", "📜", "warning") },
            { "DiaLy", ("Địa Lý", "🗺️", "secondary") },
            { "KhoaHoc", ("Khoa Học", "🔬", "purple") },
            { "DaoDuc", ("Đạo Đức", "⭐", "gold") },
            { "AmNhac", ("Âm Nhạc", "🎵", "pink") },
            { "MyThuat", ("Mỹ Thuật", "🎨", "orange") },
            { "TheDuc", ("Thể Dục", "⚽", "teal") },
            { "TinHoc", ("Tin Học", "💻", "dark") }
        };

        public static string GetName(string code) => Subjects.TryGetValue(code, out var s) ? s.Name : code;
        public static string GetIcon(string code) => Subjects.TryGetValue(code, out var s) ? s.Icon : "📖";
        public static string GetColor(string code) => Subjects.TryGetValue(code, out var s) ? s.Color : "secondary";
        
        public static List<(string Code, string Name, string Icon)> GetAll() => 
            Subjects.Select(s => (s.Key, s.Value.Name, s.Value.Icon)).ToList();
    }
}
