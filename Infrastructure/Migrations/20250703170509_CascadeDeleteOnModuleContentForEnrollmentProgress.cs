using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteOnModuleContentForEnrollmentProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentProgresses_ModuleContents_ModuleContentId",
                table: "EnrollmentProgresses");

            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentProgresses_ModuleContents_ModuleContentId",
                table: "EnrollmentProgresses",
                column: "ModuleContentId",
                principalTable: "ModuleContents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentProgresses_ModuleContents_ModuleContentId",
                table: "EnrollmentProgresses");

            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentProgresses_ModuleContents_ModuleContentId",
                table: "EnrollmentProgresses",
                column: "ModuleContentId",
                principalTable: "ModuleContents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
