using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCourseSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubjectID",
                table: "Courses",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SubjectID",
                table: "Courses",
                column: "SubjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_clsSubject_SubjectID",
                table: "Courses",
                column: "SubjectID",
                principalTable: "clsSubject",
                principalColumn: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_clsSubject_SubjectID",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "clsSubject");

            migrationBuilder.DropIndex(
                name: "IX_Courses_SubjectID",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "SubjectID",
                table: "Courses");
        }
    }
}
