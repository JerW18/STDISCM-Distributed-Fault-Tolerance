using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Course_Service.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    courseCode = table.Column<string>(type: "TEXT", nullable: false),
                    courseName = table.Column<string>(type: "TEXT", nullable: false),
                    courseSection = table.Column<string>(type: "TEXT", nullable: false),
                    units = table.Column<int>(type: "INTEGER", nullable: false),
                    capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    students = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CourseId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Courses");
        }
    }
}
