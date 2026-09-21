using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIKOCLUK.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAdviceHistoryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiAdviceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GenelDegerlendirme = table.Column<string>(type: "TEXT", nullable: false),
                    HaftalikOdakTavsiyesi = table.Column<string>(type: "TEXT", nullable: false),
                    KirmiziAlarmDersleri = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAdviceHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamTopicErrors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExamResultId = table.Column<int>(type: "INTEGER", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    TopicName = table.Column<string>(type: "TEXT", nullable: false),
                    IncorrectCount = table.Column<int>(type: "INTEGER", nullable: false),
                    BlankCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamTopicErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamTopicErrors_ExamResults_ExamResultId",
                        column: x => x.ExamResultId,
                        principalTable: "ExamResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiTargetSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AiAdviceHistoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ders = table.Column<string>(type: "TEXT", nullable: false),
                    Konu = table.Column<string>(type: "TEXT", nullable: false),
                    Neden = table.Column<string>(type: "TEXT", nullable: false),
                    Taktik = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiTargetSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiTargetSubjects_AiAdviceHistories_AiAdviceHistoryId",
                        column: x => x.AiAdviceHistoryId,
                        principalTable: "AiAdviceHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiTargetSubjects_AiAdviceHistoryId",
                table: "AiTargetSubjects",
                column: "AiAdviceHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamTopicErrors_ExamResultId",
                table: "ExamTopicErrors",
                column: "ExamResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiTargetSubjects");

            migrationBuilder.DropTable(
                name: "ExamTopicErrors");

            migrationBuilder.DropTable(
                name: "AiAdviceHistories");
        }
    }
}
