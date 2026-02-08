using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HistoryGeoQuiz_PrimarySchool.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    AnswerText = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    AnswerLabel = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClassName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Grade = table.Column<int>(type: "integer", nullable: false),
                    HomeroomTeacherId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClassRoomId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Lessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Subject = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LessonNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ClassRoomId = table.Column<int>(type: "integer", nullable: true),
                    QuestionCountForExcellent = table.Column<int>(type: "integer", nullable: true),
                    QuestionCountForGood = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lessons_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Lessons_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    ClassRoomId = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherAssignments_ClassRooms_ClassRoomId",
                        column: x => x.ClassRoomId,
                        principalTable: "ClassRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherAssignments_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: false),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeTakenSeconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestResults_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestResults_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TestResultId = table.Column<int>(type: "integer", nullable: false),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    SelectedAnswerId = table.Column<int>(type: "integer", nullable: true),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestDetails_Answers_SelectedAnswerId",
                        column: x => x.SelectedAnswerId,
                        principalTable: "Answers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestDetails_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestDetails_TestResults_TestResultId",
                        column: x => x.TestResultId,
                        principalTable: "TestResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ClassRoomId", "CreatedAt", "FullName", "Password", "Role", "Username" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quản trị hệ thống", "admin123", "Admin", "admin" },
                    { 2, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguyễn Thị Thảo", "123", "Teacher", "nguyenthuthao" },
                    { 3, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trần Thị Hiền", "123", "Teacher", "tranthihien" },
                    { 4, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lê Thị Mai", "123", "Teacher", "lethimai" },
                    { 5, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phạm Thị Lan", "123", "Teacher", "phamthilan" },
                    { 6, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đặng Thị Hạnh", "123", "Teacher", "dangthihanh" },
                    { 7, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hoàng Thị Linh", "123", "Teacher", "hoangthilinh" },
                    { 8, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vũ Thị Hương", "123", "Teacher", "vuthihuong" },
                    { 9, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bùi Thị Nam", "123", "Teacher", "buithinam" },
                    { 10, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đỗ Thị Bích", "123", "Teacher", "dothibich" },
                    { 11, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguyễn Văn Tuấn", "123", "Teacher", "nguyenvantuan" },
                    { 12, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trần Văn Long", "123", "Teacher", "tranvanlong" },
                    { 13, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lê Văn Đức", "123", "Teacher", "levanduc" },
                    { 14, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phạm Văn Minh", "123", "Teacher", "phamvanminh" },
                    { 15, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đặng Văn Khánh", "123", "Teacher", "dangvankhanh" },
                    { 16, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hoàng Văn Phúc", "123", "Teacher", "hoangvanphuc" },
                    { 17, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vũ Văn Thành", "123", "Teacher", "vuvanthanh" },
                    { 18, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bùi Văn Hùng", "123", "Teacher", "buivanhung" },
                    { 19, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đỗ Văn Quang", "123", "Teacher", "dovanquang" },
                    { 20, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguyễn Thị Hoa", "123", "Teacher", "nguyenthihoa" },
                    { 21, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trần Thị Nga", "123", "Teacher", "tranthinga" },
                    { 22, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lê Thị Phương", "123", "Teacher", "lethiphuong" },
                    { 23, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phạm Thị Oanh", "123", "Teacher", "phamthioanh" },
                    { 24, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đặng Thị Tuyết", "123", "Teacher", "dangthituyet" },
                    { 25, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hoàng Thị Thu", "123", "Teacher", "hoangthithu" },
                    { 26, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vũ Thị Tâm", "123", "Teacher", "vuthitam" },
                    { 27, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bùi Thị Yến", "123", "Teacher", "buithiyen" },
                    { 28, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đỗ Thị Ngọc", "123", "Teacher", "dothingoc" },
                    { 29, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguyễn Văn Hải", "123", "Teacher", "nguyenvanhai" },
                    { 30, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trần Văn Sơn", "123", "Teacher", "tranvanson" },
                    { 31, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lê Văn Nghĩa", "123", "Teacher", "levannghia" },
                    { 32, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phạm Văn Bình", "123", "Teacher", "phamvanbinh" },
                    { 33, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đặng Văn Dũng", "123", "Teacher", "dangvandung" },
                    { 34, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hoàng Văn Lộc", "123", "Teacher", "hoangvanloc" },
                    { 35, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vũ Văn Nam", "123", "Teacher", "vuvannam" },
                    { 36, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bùi Văn Thắng", "123", "Teacher", "buivanthang" },
                    { 37, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đỗ Văn Hiếu", "123", "Teacher", "dovanhieu" },
                    { 38, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguyễn Thị Kim", "123", "Teacher", "nguyenthikim" },
                    { 39, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trần Thị Liên", "123", "Teacher", "tranthilien" },
                    { 40, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lê Thị Châu", "123", "Teacher", "lethichau" },
                    { 41, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phạm Thị Xuân", "123", "Teacher", "phamthixuan" },
                    { 42, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đặng Thị Hà", "123", "Teacher", "dangthiha" },
                    { 43, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hoàng Thị Dung", "123", "Teacher", "hoangthidung" },
                    { 44, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vũ Thị Quyên", "123", "Teacher", "vuthiquyen" },
                    { 45, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bùi Thị Trang", "123", "Teacher", "buithitrang" },
                    { 46, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đỗ Thị Lan", "123", "Teacher", "dothilan" },
                    { 47, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đặng Thị Anh Thư", "123", "Teacher", "dangthianhthu" },
                    { 48, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lê Văn Phong", "123", "Teacher", "levanphong" },
                    { 49, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguyễn Thị Mỹ Linh", "123", "Teacher", "nguyenthimylinh" },
                    { 50, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trần Văn Kiên", "123", "Teacher", "tranvankien" },
                    { 51, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phạm Thị Hoài Anh", "123", "Teacher", "phamthihoaianh" },
                    { 52, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hoàng Văn Cường", "123", "Teacher", "hoangvancuong" },
                    { 53, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vũ Thị Thảo", "123", "Teacher", "vuthithao" },
                    { 54, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bùi Thị Kim", "123", "Teacher", "buithikim" },
                    { 55, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguyễn Văn Hùng", "123", "Teacher", "nguyenvanhung" },
                    { 56, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trần Thị Hà", "123", "Teacher", "tranthiha" },
                    { 57, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lê Thị Lan", "123", "Teacher", "lethilan" },
                    { 58, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phạm Văn Tài", "123", "Teacher", "phamvantai" },
                    { 59, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đặng Thị Hương", "123", "Teacher", "dangthihuong" },
                    { 60, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hoàng Thị Phương", "123", "Teacher", "hoangthiphuong" },
                    { 61, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vũ Văn Nhân", "123", "Teacher", "vuvannhan" },
                    { 62, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguyễn Văn Duy", "123", "Teacher", "nguyenvanduy" },
                    { 63, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trần Thị Hằng", "123", "Teacher", "tranthihang" },
                    { 64, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lê Thị Thu", "123", "Teacher", "lethithu" },
                    { 65, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phạm Văn Minh", "123", "Teacher", "phamvanminh2" },
                    { 66, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đặng Văn Tùng", "123", "Teacher", "dangvantung" },
                    { 67, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hoàng Thị Anh", "123", "Teacher", "hoangthianh" },
                    { 68, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vũ Văn Bảo", "123", "Teacher", "vuvanbao" },
                    { 69, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bùi Thị Châu", "123", "Teacher", "buithichau" },
                    { 70, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nguyễn Văn Đạt", "123", "Teacher", "nguyenvandat" },
                    { 71, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trần Thị Liên", "123", "Teacher", "tranthilien2" },
                    { 72, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lê Thị Mai", "123", "Teacher", "lethimai2" },
                    { 73, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phạm Văn Nam", "123", "Teacher", "phamvannam" },
                    { 74, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Đặng Thị Ngọc", "123", "Teacher", "dangthingoc" },
                    { 75, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hoàng Văn Tiến", "123", "Teacher", "hoangvantien" },
                    { 76, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vũ Thị Huyền", "123", "Teacher", "vuthihuyen" }
                });

            migrationBuilder.InsertData(
                table: "ClassRooms",
                columns: new[] { "Id", "ClassName", "CreatedAt", "Grade", "HomeroomTeacherId" },
                values: new object[,]
                {
                    { 1, "1/1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2 },
                    { 2, "1/2", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 3 },
                    { 3, "1/3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 4 },
                    { 4, "1/4", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 5 },
                    { 5, "1/5", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 6 },
                    { 6, "1/6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 7 },
                    { 7, "1/7", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 8 },
                    { 8, "1/8", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 9 },
                    { 9, "1/9", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 10 },
                    { 10, "2/1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 11 },
                    { 11, "2/2", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 12 },
                    { 12, "2/3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 13 },
                    { 13, "2/4", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 14 },
                    { 14, "2/5", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 15 },
                    { 15, "2/6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 16 },
                    { 16, "2/7", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 17 },
                    { 17, "2/8", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 18 },
                    { 18, "2/9", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 19 },
                    { 19, "3/1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 20 },
                    { 20, "3/2", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 21 },
                    { 21, "3/3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 22 },
                    { 22, "3/4", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 23 },
                    { 23, "3/5", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 24 },
                    { 24, "3/6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 25 },
                    { 25, "3/7", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 26 },
                    { 26, "3/8", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 27 },
                    { 27, "3/9", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 28 },
                    { 28, "4/1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 29 },
                    { 29, "4/2", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 30 },
                    { 30, "4/3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 31 },
                    { 31, "4/4", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 32 },
                    { 32, "4/5", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 33 },
                    { 33, "4/6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 34 },
                    { 34, "4/7", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 35 },
                    { 35, "4/8", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 36 },
                    { 36, "4/9", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 37 },
                    { 37, "5/1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 38 },
                    { 38, "5/2", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 39 },
                    { 39, "5/3", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 40 },
                    { 40, "5/4", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 41 },
                    { 41, "5/5", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 42 },
                    { 42, "5/6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 43 },
                    { 43, "5/7", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 44 },
                    { 44, "5/8", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 45 },
                    { 45, "5/9", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 46 }
                });

            migrationBuilder.InsertData(
                table: "TeacherAssignments",
                columns: new[] { "Id", "AssignedDate", "ClassRoomId", "Subject", "TeacherId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Toan", 2 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "TiengViet", 2 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Toan", 3 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "TiengViet", 3 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Toan", 4 },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "TiengViet", 4 },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Toan", 5 },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "TiengViet", 5 },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "Toan", 6 },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "TiengViet", 6 },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "Toan", 7 },
                    { 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "TiengViet", 7 },
                    { 13, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "Toan", 8 },
                    { 14, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "TiengViet", 8 },
                    { 15, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "Toan", 9 },
                    { 16, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "TiengViet", 9 },
                    { 17, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9, "Toan", 10 },
                    { 18, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9, "TiengViet", 10 },
                    { 19, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, "Toan", 11 },
                    { 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, "TiengViet", 11 },
                    { 21, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11, "Toan", 12 },
                    { 22, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11, "TiengViet", 12 },
                    { 23, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12, "Toan", 13 },
                    { 24, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12, "TiengViet", 13 },
                    { 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 13, "Toan", 14 },
                    { 26, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 13, "TiengViet", 14 },
                    { 27, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 14, "Toan", 15 },
                    { 28, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 14, "TiengViet", 15 },
                    { 29, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15, "Toan", 16 },
                    { 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15, "TiengViet", 16 },
                    { 31, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 16, "Toan", 17 },
                    { 32, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 16, "TiengViet", 17 },
                    { 33, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, "Toan", 18 },
                    { 34, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, "TiengViet", 18 },
                    { 35, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18, "Toan", 19 },
                    { 36, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18, "TiengViet", 19 },
                    { 37, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 19, "Toan", 20 },
                    { 38, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 19, "TiengViet", 20 },
                    { 39, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 20, "Toan", 21 },
                    { 40, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 20, "TiengViet", 21 },
                    { 41, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 21, "Toan", 22 },
                    { 42, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 21, "TiengViet", 22 },
                    { 43, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, "Toan", 23 },
                    { 44, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, "TiengViet", 23 },
                    { 45, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 23, "Toan", 24 },
                    { 46, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 23, "TiengViet", 24 },
                    { 47, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, "Toan", 25 },
                    { 48, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, "TiengViet", 25 },
                    { 49, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25, "Toan", 26 },
                    { 50, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25, "TiengViet", 26 },
                    { 51, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 26, "Toan", 27 },
                    { 52, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 26, "TiengViet", 27 },
                    { 53, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, "Toan", 28 },
                    { 54, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, "TiengViet", 28 },
                    { 55, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 28, "Toan", 29 },
                    { 56, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 28, "TiengViet", 29 },
                    { 57, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, "Toan", 30 },
                    { 58, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, "TiengViet", 30 },
                    { 59, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, "Toan", 31 },
                    { 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, "TiengViet", 31 },
                    { 61, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, "Toan", 32 },
                    { 62, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, "TiengViet", 32 },
                    { 63, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 32, "Toan", 33 },
                    { 64, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 32, "TiengViet", 33 },
                    { 65, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, "Toan", 34 },
                    { 66, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, "TiengViet", 34 },
                    { 67, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 34, "Toan", 35 },
                    { 68, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 34, "TiengViet", 35 },
                    { 69, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, "Toan", 36 },
                    { 70, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, "TiengViet", 36 },
                    { 71, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 36, "Toan", 37 },
                    { 72, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 36, "TiengViet", 37 },
                    { 73, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, "Toan", 38 },
                    { 74, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, "TiengViet", 38 },
                    { 75, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 38, "Toan", 39 },
                    { 76, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 38, "TiengViet", 39 },
                    { 77, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 39, "Toan", 40 },
                    { 78, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 39, "TiengViet", 40 },
                    { 79, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 40, "Toan", 41 },
                    { 80, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 40, "TiengViet", 41 },
                    { 81, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 41, "Toan", 42 },
                    { 82, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 41, "TiengViet", 42 },
                    { 83, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 42, "Toan", 43 },
                    { 84, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 42, "TiengViet", 43 },
                    { 85, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, "Toan", 44 },
                    { 86, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, "TiengViet", 44 },
                    { 87, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 44, "Toan", 45 },
                    { 88, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 44, "TiengViet", 45 },
                    { 89, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, "Toan", 46 },
                    { 90, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, "TiengViet", 46 },
                    { 91, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "TiengAnh", 47 },
                    { 92, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "TiengAnh", 47 },
                    { 93, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "TiengAnh", 47 },
                    { 94, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "TiengAnh", 47 },
                    { 95, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "TiengAnh", 47 },
                    { 96, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "TiengAnh", 47 },
                    { 97, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "TiengAnh", 47 },
                    { 98, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "TiengAnh", 47 },
                    { 99, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9, "TiengAnh", 47 },
                    { 100, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, "TiengAnh", 48 },
                    { 101, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11, "TiengAnh", 48 },
                    { 102, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12, "TiengAnh", 48 },
                    { 103, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 13, "TiengAnh", 48 },
                    { 104, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 14, "TiengAnh", 48 },
                    { 105, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15, "TiengAnh", 48 },
                    { 106, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 16, "TiengAnh", 48 },
                    { 107, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, "TiengAnh", 48 },
                    { 108, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18, "TiengAnh", 48 },
                    { 109, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 19, "TiengAnh", 49 },
                    { 110, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 20, "TiengAnh", 49 },
                    { 111, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 21, "TiengAnh", 49 },
                    { 112, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, "TiengAnh", 49 },
                    { 113, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 23, "TiengAnh", 49 },
                    { 114, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, "TiengAnh", 49 },
                    { 115, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25, "TiengAnh", 49 },
                    { 116, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 26, "TiengAnh", 49 },
                    { 117, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, "TiengAnh", 49 },
                    { 118, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 28, "TiengAnh", 50 },
                    { 119, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, "TiengAnh", 50 },
                    { 120, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, "TiengAnh", 50 },
                    { 121, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, "TiengAnh", 50 },
                    { 122, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 32, "TiengAnh", 50 },
                    { 123, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, "TiengAnh", 50 },
                    { 124, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 34, "TiengAnh", 50 },
                    { 125, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, "TiengAnh", 50 },
                    { 126, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 36, "TiengAnh", 50 },
                    { 127, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, "TiengAnh", 51 },
                    { 128, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 38, "TiengAnh", 51 },
                    { 129, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 39, "TiengAnh", 51 },
                    { 130, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 40, "TiengAnh", 51 },
                    { 131, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 41, "TiengAnh", 51 },
                    { 132, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 42, "TiengAnh", 51 },
                    { 133, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, "TiengAnh", 51 },
                    { 134, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 44, "TiengAnh", 51 },
                    { 135, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, "TiengAnh", 51 },
                    { 136, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "TNXH", 52 },
                    { 137, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "TNXH", 52 },
                    { 138, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "TNXH", 52 },
                    { 139, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "TNXH", 52 },
                    { 140, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "TNXH", 52 },
                    { 141, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "TNXH", 52 },
                    { 142, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "TNXH", 52 },
                    { 143, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "TNXH", 52 },
                    { 144, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9, "TNXH", 52 },
                    { 145, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, "TNXH", 53 },
                    { 146, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11, "TNXH", 53 },
                    { 147, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12, "TNXH", 53 },
                    { 148, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 13, "TNXH", 53 },
                    { 149, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 14, "TNXH", 53 },
                    { 150, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15, "TNXH", 53 },
                    { 151, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 16, "TNXH", 53 },
                    { 152, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, "TNXH", 53 },
                    { 153, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18, "TNXH", 53 },
                    { 154, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 19, "TNXH", 54 },
                    { 155, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 20, "TNXH", 54 },
                    { 156, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 21, "TNXH", 54 },
                    { 157, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, "TNXH", 54 },
                    { 158, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 23, "TNXH", 54 },
                    { 159, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, "TNXH", 54 },
                    { 160, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25, "TNXH", 54 },
                    { 161, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 26, "TNXH", 54 },
                    { 162, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, "TNXH", 54 },
                    { 163, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 28, "TNXH", 55 },
                    { 164, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, "TNXH", 55 },
                    { 165, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, "TNXH", 55 },
                    { 166, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, "TNXH", 55 },
                    { 167, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 32, "TNXH", 55 },
                    { 168, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, "TNXH", 55 },
                    { 169, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 34, "TNXH", 55 },
                    { 170, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, "TNXH", 55 },
                    { 171, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 36, "TNXH", 55 },
                    { 172, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, "TNXH", 56 },
                    { 173, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 38, "TNXH", 56 },
                    { 174, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 39, "TNXH", 56 },
                    { 175, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 40, "TNXH", 56 },
                    { 176, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 41, "TNXH", 56 },
                    { 177, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 42, "TNXH", 56 },
                    { 178, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, "TNXH", 56 },
                    { 179, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 44, "TNXH", 56 },
                    { 180, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, "TNXH", 56 },
                    { 181, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "DaoDuc", 57 },
                    { 182, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "DaoDuc", 57 },
                    { 183, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "DaoDuc", 57 },
                    { 184, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "DaoDuc", 57 },
                    { 185, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "DaoDuc", 57 },
                    { 186, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "DaoDuc", 57 },
                    { 187, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "DaoDuc", 57 },
                    { 188, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "DaoDuc", 57 },
                    { 189, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9, "DaoDuc", 57 },
                    { 190, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, "DaoDuc", 58 },
                    { 191, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11, "DaoDuc", 58 },
                    { 192, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12, "DaoDuc", 58 },
                    { 193, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 13, "DaoDuc", 58 },
                    { 194, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 14, "DaoDuc", 58 },
                    { 195, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15, "DaoDuc", 58 },
                    { 196, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 16, "DaoDuc", 58 },
                    { 197, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, "DaoDuc", 58 },
                    { 198, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18, "DaoDuc", 58 },
                    { 199, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 19, "DaoDuc", 59 },
                    { 200, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 20, "DaoDuc", 59 },
                    { 201, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 21, "DaoDuc", 59 },
                    { 202, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, "DaoDuc", 59 },
                    { 203, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 23, "DaoDuc", 59 },
                    { 204, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, "DaoDuc", 59 },
                    { 205, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25, "DaoDuc", 59 },
                    { 206, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 26, "DaoDuc", 59 },
                    { 207, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, "DaoDuc", 59 },
                    { 208, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 28, "DaoDuc", 60 },
                    { 209, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, "DaoDuc", 60 },
                    { 210, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, "DaoDuc", 60 },
                    { 211, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, "DaoDuc", 60 },
                    { 212, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 32, "DaoDuc", 60 },
                    { 213, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, "DaoDuc", 60 },
                    { 214, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 34, "DaoDuc", 60 },
                    { 215, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, "DaoDuc", 60 },
                    { 216, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 36, "DaoDuc", 60 },
                    { 217, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, "DaoDuc", 61 },
                    { 218, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 38, "DaoDuc", 61 },
                    { 219, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 39, "DaoDuc", 61 },
                    { 220, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 40, "DaoDuc", 61 },
                    { 221, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 41, "DaoDuc", 61 },
                    { 222, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 42, "DaoDuc", 61 },
                    { 223, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, "DaoDuc", 61 },
                    { 224, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 44, "DaoDuc", 61 },
                    { 225, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, "DaoDuc", 61 },
                    { 226, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "TinHoc", 62 },
                    { 227, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "TinHoc", 62 },
                    { 228, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "TinHoc", 62 },
                    { 229, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "TinHoc", 62 },
                    { 230, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "TinHoc", 62 },
                    { 231, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "TinHoc", 62 },
                    { 232, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "TinHoc", 62 },
                    { 233, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "TinHoc", 62 },
                    { 234, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9, "TinHoc", 62 },
                    { 235, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, "TinHoc", 63 },
                    { 236, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11, "TinHoc", 63 },
                    { 237, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12, "TinHoc", 63 },
                    { 238, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 13, "TinHoc", 63 },
                    { 239, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 14, "TinHoc", 63 },
                    { 240, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15, "TinHoc", 63 },
                    { 241, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 16, "TinHoc", 63 },
                    { 242, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, "TinHoc", 63 },
                    { 243, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18, "TinHoc", 63 },
                    { 244, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 19, "TinHoc", 64 },
                    { 245, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 20, "TinHoc", 64 },
                    { 246, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 21, "TinHoc", 64 },
                    { 247, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, "TinHoc", 64 },
                    { 248, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 23, "TinHoc", 64 },
                    { 249, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, "TinHoc", 64 },
                    { 250, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25, "TinHoc", 64 },
                    { 251, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 26, "TinHoc", 64 },
                    { 252, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, "TinHoc", 64 },
                    { 253, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 28, "TinHoc", 65 },
                    { 254, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, "TinHoc", 65 },
                    { 255, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, "TinHoc", 65 },
                    { 256, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, "TinHoc", 65 },
                    { 257, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 32, "TinHoc", 65 },
                    { 258, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, "TinHoc", 65 },
                    { 259, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 34, "TinHoc", 65 },
                    { 260, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, "TinHoc", 65 },
                    { 261, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 36, "TinHoc", 65 },
                    { 262, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, "TinHoc", 66 },
                    { 263, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 38, "TinHoc", 66 },
                    { 264, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 39, "TinHoc", 66 },
                    { 265, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 40, "TinHoc", 66 },
                    { 266, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 41, "TinHoc", 66 },
                    { 267, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 42, "TinHoc", 66 },
                    { 268, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, "TinHoc", 66 },
                    { 269, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 44, "TinHoc", 66 },
                    { 270, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, "TinHoc", 66 },
                    { 271, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "MyThuat", 67 },
                    { 272, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "MyThuat", 67 },
                    { 273, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "MyThuat", 67 },
                    { 274, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "MyThuat", 67 },
                    { 275, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "MyThuat", 67 },
                    { 276, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "MyThuat", 67 },
                    { 277, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "MyThuat", 67 },
                    { 278, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "MyThuat", 67 },
                    { 279, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9, "MyThuat", 67 },
                    { 280, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, "MyThuat", 68 },
                    { 281, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11, "MyThuat", 68 },
                    { 282, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12, "MyThuat", 68 },
                    { 283, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 13, "MyThuat", 68 },
                    { 284, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 14, "MyThuat", 68 },
                    { 285, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15, "MyThuat", 68 },
                    { 286, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 16, "MyThuat", 68 },
                    { 287, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, "MyThuat", 68 },
                    { 288, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18, "MyThuat", 68 },
                    { 289, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 19, "MyThuat", 69 },
                    { 290, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 20, "MyThuat", 69 },
                    { 291, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 21, "MyThuat", 69 },
                    { 292, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, "MyThuat", 69 },
                    { 293, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 23, "MyThuat", 69 },
                    { 294, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, "MyThuat", 69 },
                    { 295, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25, "MyThuat", 69 },
                    { 296, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 26, "MyThuat", 69 },
                    { 297, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, "MyThuat", 69 },
                    { 298, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 28, "MyThuat", 70 },
                    { 299, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, "MyThuat", 70 },
                    { 300, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, "MyThuat", 70 },
                    { 301, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, "MyThuat", 70 },
                    { 302, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 32, "MyThuat", 70 },
                    { 303, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, "MyThuat", 70 },
                    { 304, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 34, "MyThuat", 70 },
                    { 305, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, "MyThuat", 70 },
                    { 306, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 36, "MyThuat", 70 },
                    { 307, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, "MyThuat", 71 },
                    { 308, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 38, "MyThuat", 71 },
                    { 309, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 39, "MyThuat", 71 },
                    { 310, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 40, "MyThuat", 71 },
                    { 311, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 41, "MyThuat", 71 },
                    { 312, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 42, "MyThuat", 71 },
                    { 313, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, "MyThuat", 71 },
                    { 314, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 44, "MyThuat", 71 },
                    { 315, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, "MyThuat", 71 },
                    { 316, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "KhoaHoc", 72 },
                    { 317, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "KhoaHoc", 72 },
                    { 318, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "KhoaHoc", 72 },
                    { 319, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "KhoaHoc", 72 },
                    { 320, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "KhoaHoc", 72 },
                    { 321, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "KhoaHoc", 72 },
                    { 322, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "KhoaHoc", 72 },
                    { 323, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "KhoaHoc", 72 },
                    { 324, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9, "KhoaHoc", 72 },
                    { 325, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, "KhoaHoc", 73 },
                    { 326, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 11, "KhoaHoc", 73 },
                    { 327, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12, "KhoaHoc", 73 },
                    { 328, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 13, "KhoaHoc", 73 },
                    { 329, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 14, "KhoaHoc", 73 },
                    { 330, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 15, "KhoaHoc", 73 },
                    { 331, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 16, "KhoaHoc", 73 },
                    { 332, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 17, "KhoaHoc", 73 },
                    { 333, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 18, "KhoaHoc", 73 },
                    { 334, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 19, "KhoaHoc", 74 },
                    { 335, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 20, "KhoaHoc", 74 },
                    { 336, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 21, "KhoaHoc", 74 },
                    { 337, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 22, "KhoaHoc", 74 },
                    { 338, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 23, "KhoaHoc", 74 },
                    { 339, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 24, "KhoaHoc", 74 },
                    { 340, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25, "KhoaHoc", 74 },
                    { 341, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 26, "KhoaHoc", 74 },
                    { 342, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 27, "KhoaHoc", 74 },
                    { 343, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 28, "KhoaHoc", 75 },
                    { 344, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 29, "KhoaHoc", 75 },
                    { 345, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, "KhoaHoc", 75 },
                    { 346, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 31, "KhoaHoc", 75 },
                    { 347, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 32, "KhoaHoc", 75 },
                    { 348, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 33, "KhoaHoc", 75 },
                    { 349, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 34, "KhoaHoc", 75 },
                    { 350, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 35, "KhoaHoc", 75 },
                    { 351, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 36, "KhoaHoc", 75 },
                    { 352, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 37, "KhoaHoc", 76 },
                    { 353, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 38, "KhoaHoc", 76 },
                    { 354, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 39, "KhoaHoc", 76 },
                    { 355, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 40, "KhoaHoc", 76 },
                    { 356, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 41, "KhoaHoc", 76 },
                    { 357, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 42, "KhoaHoc", 76 },
                    { 358, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 43, "KhoaHoc", 76 },
                    { 359, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 44, "KhoaHoc", 76 },
                    { 360, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 45, "KhoaHoc", 76 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassRooms_HomeroomTeacherId",
                table: "ClassRooms",
                column: "HomeroomTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_ClassRoomId",
                table: "Lessons",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_CreatedByUserId",
                table: "Lessons",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_LessonId",
                table: "Questions",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAssignments_ClassRoomId",
                table: "TeacherAssignments",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAssignments_TeacherId",
                table: "TeacherAssignments",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TestDetails_QuestionId",
                table: "TestDetails",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestDetails_SelectedAnswerId",
                table: "TestDetails",
                column: "SelectedAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_TestDetails_TestResultId",
                table: "TestDetails",
                column: "TestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_LessonId",
                table: "TestResults",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_StudentId",
                table: "TestResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClassRoomId",
                table: "Users",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassRooms_Users_HomeroomTeacherId",
                table: "ClassRooms",
                column: "HomeroomTeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassRooms_Users_HomeroomTeacherId",
                table: "ClassRooms");

            migrationBuilder.DropTable(
                name: "TeacherAssignments");

            migrationBuilder.DropTable(
                name: "TestDetails");

            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "TestResults");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Lessons");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ClassRooms");
        }
    }
}
