using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCourseLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LanguageID",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Language",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Language", x => x.LanguageId);
                });

            migrationBuilder.InsertData(
                table: "Language",
                columns: new[] { "LanguageId", "Name" },
                values: new object[,]
                {
                    { 1, "English" },
                    { 2, "Spanish" },
                    { 3, "French" },
                    { 4, "Arabic" },
                    { 5, "Portuguese (Brazil)" },
                    { 6, "German" },
                    { 7, "Chinese (China)" },
                    { 8, "Japanese" },
                    { 9, "Indonesian" },
                    { 10, "Russian" },
                    { 11, "Korean" },
                    { 12, "Hindi" },
                    { 13, "Turkish" },
                    { 14, "Ukrainian" },
                    { 15, "Italian" },
                    { 16, "Thai" },
                    { 17, "Polish" },
                    { 18, "Dutch" },
                    { 19, "Swedish" },
                    { 20, "Greek" },
                    { 21, "Kazakh" },
                    { 22, "Hungarian" },
                    { 23, "Azerbaijani" },
                    { 24, "Vietnamese" },
                    { 25, "Pushto" },
                    { 26, "Chinese (Traditional)" },
                    { 27, "Hebrew" },
                    { 28, "Portuguese" },
                    { 29, "Portuguese (Portugal)" },
                    { 30, "Catalan" },
                    { 31, "Croatian" },
                    { 32, "Kannada" },
                    { 33, "Swahili" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_LanguageID",
                table: "Courses",
                column: "LanguageID");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Language_LanguageID",
                table: "Courses",
                column: "LanguageID",
                principalTable: "Language",
                principalColumn: "LanguageId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Language_LanguageID",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "Language");

            migrationBuilder.DropIndex(
                name: "IX_Courses_LanguageID",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "LanguageID",
                table: "Courses");
        }
    }
}
