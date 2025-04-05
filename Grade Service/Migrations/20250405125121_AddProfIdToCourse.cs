using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grade_Service.Migrations
{
    /// <inheritdoc />
    public partial class AddProfIdToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CourseName",
                table: "Grades",
                newName: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Grades",
                newName: "CourseName");
        }
    }
}
