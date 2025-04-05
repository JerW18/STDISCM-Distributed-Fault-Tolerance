using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Course_Service.Migrations
{
    /// <inheritdoc />
    public partial class AddProfIdToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "units",
                table: "Courses",
                newName: "Units");

            migrationBuilder.RenameColumn(
                name: "students",
                table: "Courses",
                newName: "Students");

            migrationBuilder.RenameColumn(
                name: "courseSection",
                table: "Courses",
                newName: "CourseSection");

            migrationBuilder.RenameColumn(
                name: "courseName",
                table: "Courses",
                newName: "CourseName");

            migrationBuilder.RenameColumn(
                name: "courseCode",
                table: "Courses",
                newName: "CourseCode");

            migrationBuilder.RenameColumn(
                name: "capacity",
                table: "Courses",
                newName: "Capacity");

            migrationBuilder.AddColumn<int>(
                name: "ProfId",
                table: "Courses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfId",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "Units",
                table: "Courses",
                newName: "units");

            migrationBuilder.RenameColumn(
                name: "Students",
                table: "Courses",
                newName: "students");

            migrationBuilder.RenameColumn(
                name: "CourseSection",
                table: "Courses",
                newName: "courseSection");

            migrationBuilder.RenameColumn(
                name: "CourseName",
                table: "Courses",
                newName: "courseName");

            migrationBuilder.RenameColumn(
                name: "CourseCode",
                table: "Courses",
                newName: "courseCode");

            migrationBuilder.RenameColumn(
                name: "Capacity",
                table: "Courses",
                newName: "capacity");
        }
    }
}
