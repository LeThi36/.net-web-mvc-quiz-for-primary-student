using HistoryGeoQuiz_PrimarySchool.Helpers;

namespace HistoryGeoQuiz_PrimarySchool.Models
{
    /// <summary>
    /// Base entity class providing common fields for all domain models.
    /// All entities should inherit from this class.
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTimeHelper.GetVietnamTime();

        /// <summary>Soft delete flag. When true, entity is logically deleted.</summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>Timestamp of when the entity was soft-deleted.</summary>
        public DateTime? DeletedAt { get; set; }
    }
}
