using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizPersistenceAndProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiGeneratedQuizzes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    GeneratedByUserId = table.Column<int>(type: "int", nullable: false),
                    GeneratedForEnrollmentId = table.Column<int>(type: "int", nullable: true),
                    Output = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    QuestionsCount = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false, defaultValue: "en"),
                    IncludeExplanations = table.Column<bool>(type: "bit", nullable: false),
                    GenerationSource = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Student"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiGeneratedQuizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiGeneratedQuizzes_CourseModules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "CourseModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AiGeneratedQuizzes_Enrollments_GeneratedForEnrollmentId",
                        column: x => x.GeneratedForEnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AiGeneratedQuizzes_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AiGeneratedQuizId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    AssignedByInstructorId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    DueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizAssignments_AiGeneratedQuizzes_AiGeneratedQuizId",
                        column: x => x.AiGeneratedQuizId,
                        principalTable: "AiGeneratedQuizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAssignments_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAssignments_Users_AssignedByInstructorId",
                        column: x => x.AssignedByInstructorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentQuizAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AiGeneratedQuizId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    QuizAssignmentId = table.Column<int>(type: "int", nullable: true),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    StudentResponses = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "int", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentQuizAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentQuizAttempts_AiGeneratedQuizzes_AiGeneratedQuizId",
                        column: x => x.AiGeneratedQuizId,
                        principalTable: "AiGeneratedQuizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentQuizAttempts_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentQuizAttempts_QuizAssignments_QuizAssignmentId",
                        column: x => x.QuizAssignmentId,
                        principalTable: "QuizAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiGeneratedQuizzes_GeneratedByUserId",
                table: "AiGeneratedQuizzes",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AiGeneratedQuizzes_GeneratedForEnrollmentId",
                table: "AiGeneratedQuizzes",
                column: "GeneratedForEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AiGeneratedQuizzes_ModuleId",
                table: "AiGeneratedQuizzes",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAssignments_AiGeneratedQuizId_EnrollmentId",
                table: "QuizAssignments",
                columns: new[] { "AiGeneratedQuizId", "EnrollmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizAssignments_AssignedByInstructorId",
                table: "QuizAssignments",
                column: "AssignedByInstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAssignments_EnrollmentId",
                table: "QuizAssignments",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentQuizAttempts_AiGeneratedQuizId",
                table: "StudentQuizAttempts",
                column: "AiGeneratedQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentQuizAttempts_EnrollmentId_AiGeneratedQuizId_CreatedAt",
                table: "StudentQuizAttempts",
                columns: new[] { "EnrollmentId", "AiGeneratedQuizId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentQuizAttempts_QuizAssignmentId",
                table: "StudentQuizAttempts",
                column: "QuizAssignmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentQuizAttempts");

            migrationBuilder.DropTable(
                name: "QuizAssignments");

            migrationBuilder.DropTable(
                name: "AiGeneratedQuizzes");
        }
    }
}
