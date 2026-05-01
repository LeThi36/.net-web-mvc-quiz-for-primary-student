using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoryGeoQuiz_PrimarySchool.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeLimitToLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeLimitMinutes",
                table: "Lessons",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeLimitMinutes",
                table: "Lessons");
        }
    }
}
