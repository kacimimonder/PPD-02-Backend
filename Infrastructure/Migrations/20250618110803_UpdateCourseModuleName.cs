using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCourseModuleName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseModule_Courses_CourseId",
                table: "CourseModule");

            migrationBuilder.DropForeignKey(
                name: "FK_ModuleContent_CourseModule_CourseModuleID",
                table: "ModuleContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModuleContent",
                table: "ModuleContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseModule",
                table: "CourseModule");

            migrationBuilder.RenameTable(
                name: "ModuleContent",
                newName: "ModuleContents");

            migrationBuilder.RenameTable(
                name: "CourseModule",
                newName: "CourseModules");

            migrationBuilder.RenameIndex(
                name: "IX_ModuleContent_CourseModuleID",
                table: "ModuleContents",
                newName: "IX_ModuleContents_CourseModuleID");

            migrationBuilder.RenameIndex(
                name: "IX_CourseModule_CourseId",
                table: "CourseModules",
                newName: "IX_CourseModules_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModuleContents",
                table: "ModuleContents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseModules",
                table: "CourseModules",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseModules_Courses_CourseId",
                table: "CourseModules",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleContents_CourseModules_CourseModuleID",
                table: "ModuleContents",
                column: "CourseModuleID",
                principalTable: "CourseModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseModules_Courses_CourseId",
                table: "CourseModules");

            migrationBuilder.DropForeignKey(
                name: "FK_ModuleContents_CourseModules_CourseModuleID",
                table: "ModuleContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ModuleContents",
                table: "ModuleContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseModules",
                table: "CourseModules");

            migrationBuilder.RenameTable(
                name: "ModuleContents",
                newName: "ModuleContent");

            migrationBuilder.RenameTable(
                name: "CourseModules",
                newName: "CourseModule");

            migrationBuilder.RenameIndex(
                name: "IX_ModuleContents_CourseModuleID",
                table: "ModuleContent",
                newName: "IX_ModuleContent_CourseModuleID");

            migrationBuilder.RenameIndex(
                name: "IX_CourseModules_CourseId",
                table: "CourseModule",
                newName: "IX_CourseModule_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ModuleContent",
                table: "ModuleContent",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseModule",
                table: "CourseModule",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseModule_Courses_CourseId",
                table: "CourseModule",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleContent_CourseModule_CourseModuleID",
                table: "ModuleContent",
                column: "CourseModuleID",
                principalTable: "CourseModule",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
