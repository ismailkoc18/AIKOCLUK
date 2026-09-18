using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIKOCLUK.Migrations
{
    /// <inheritdoc />
    public partial class AddAiFeedbackAndStudentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TargetUniversity = table.Column<string>(type: "TEXT", nullable: false),
                    TargetDepartment = table.Column<string>(type: "TEXT", nullable: false),
                    Field = table.Column<string>(type: "TEXT", nullable: false),
                    LearningStyle = table.Column<string>(type: "TEXT", nullable: false),
                    DailyAvailableStudyHours = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentStressLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    WeakSubjects = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiFeedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GeneratedAdvice = table.Column<string>(type: "TEXT", nullable: false),
                    StudentNote = table.Column<string>(type: "TEXT", nullable: false),
                    DetectedSentiment = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiFeedbacks_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExamType = table.Column<string>(type: "TEXT", nullable: false),
                    ExamDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TurkishNet = table.Column<double>(type: "REAL", nullable: false),
                    MathNet = table.Column<double>(type: "REAL", nullable: false),
                    ScienceNet = table.Column<double>(type: "REAL", nullable: false),
                    SocialNet = table.Column<double>(type: "REAL", nullable: false),
                    TimeManagementIssue = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamResults_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiFeedbacks_StudentId",
                table: "AiFeedbacks",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_StudentId",
                table: "ExamResults",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiFeedbacks");

            migrationBuilder.DropTable(
                name: "ExamResults");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
