using System;

namespace HistoryGeoQuiz_PrimarySchool.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        /// <summary>
        /// Lấy thời gian hiện tại theo múi giờ Việt Nam (UTC+7)
        /// </summary>
        public static DateTime GetVietnamTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
        }

        /// <summary>
        /// Chuyển đổi một DateTime bất kỳ sang giờ Việt Nam
        /// </summary>
        public static DateTime ToVietnamTime(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, VietnamTimeZone);
            }
            
            // Nếu đã là Local hoặc Unspecified, giả định là đang ở UTC và chuyển đổi (hoặc giữ nguyên nếu đã là VN)
            // Tuy nhiên, an toàn nhất là chuyển qua UTC rồi mới sang VN
            return TimeZoneInfo.ConvertTime(dateTime, VietnamTimeZone);
        }
    }
}
