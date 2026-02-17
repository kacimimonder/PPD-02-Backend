using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCourseSubjectWithoutClsKeword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_clsSubject_SubjectID",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Courses_clsCourseId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "clsSubject");

            migrationBuilder.RenameColumn(
                name: "clsCourseId",
                table: "Students",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_clsCourseId",
                table: "Students",
                newName: "IX_Students_CourseId");

            migrationBuilder.CreateTable(
                name: "Subject",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subject", x => x.SubjectId);
                });

            migrationBuilder.InsertData(
                table: "Subject",
                columns: new[] { "SubjectId", "Name" },
                values: new object[,]
                {
                    { 1, "Business" },
                    { 2, "Computer Science" },
                    { 3, "Information Technology" },
                    { 4, "Data Science" },
                    { 5, "Health" },
                    { 6, "Physical Science and Engineering" },
                    { 7, "Social Sciences" },
                    { 8, "Arts and Humanities" },
                    { 9, "Personal Development" },
                    { 10, "Language Learning" },
                    { 11, "Math and Logic" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Subject_SubjectID",
                table: "Courses",
                column: "SubjectID",
                principalTable: "Subject",
                principalColumn: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Courses_CourseId",
                table: "Students",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Subject_SubjectID",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Courses_CourseId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Subject");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Students",
                newName: "clsCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_CourseId",
                table: "Students",
                newName: "IX_Students_clsCourseId");

            migrationBuilder.CreateTable(
                name: "clsSubject",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clsSubject", x => x.SubjectId);
                });

            migrationBuilder.InsertData(
                table: "clsSubject",
                columns: new[] { "SubjectId", "Name" },
                values: new object[,]
                {
                    { 1, "Business" },
                    { 2, "Computer Science" },
                    { 3, "Information Technology" },
                    { 4, "Data Science" },
                    { 5, "Health" },
                    { 6, "Physical Science and Engineering" },
                    { 7, "Social Sciences" },
                    { 8, "Arts and Humanities" },
                    { 9, "Personal Development" },
                    { 10, "Language Learning" },
                    { 11, "Math and Logic" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_clsSubject_SubjectID",
                table: "Courses",
                column: "SubjectID",
                principalTable: "clsSubject",
                principalColumn: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Courses_clsCourseId",
                table: "Students",
                column: "clsCourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }
    }
}
