using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grade_Service.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGradeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseCode",
                table: "Grades",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Firstname",
                table: "Grades",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Lastname",
                table: "Grades",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseCode",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "Firstname",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "Lastname",
                table: "Grades");
        }
    }
}
