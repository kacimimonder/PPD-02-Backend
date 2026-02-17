using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAfterReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentProgresses_Enrollments_EnrollmentId",
                table: "EnrollmentProgresses");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentProgresses_ModuleContentId",
                table: "EnrollmentProgresses",
                column: "ModuleContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentProgresses_Enrollments_EnrollmentId",
                table: "EnrollmentProgresses",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentProgresses_ModuleContents_ModuleContentId",
                table: "EnrollmentProgresses",
                column: "ModuleContentId",
                principalTable: "ModuleContents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentProgresses_Enrollments_EnrollmentId",
                table: "EnrollmentProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentProgresses_ModuleContents_ModuleContentId",
                table: "EnrollmentProgresses");

            migrationBuilder.DropIndex(
                name: "IX_EnrollmentProgresses_ModuleContentId",
                table: "EnrollmentProgresses");

            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentProgresses_Enrollments_EnrollmentId",
                table: "EnrollmentProgresses",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
