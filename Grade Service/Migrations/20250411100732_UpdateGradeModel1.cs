using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grade_Service.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGradeModel1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Units",
                table: "Grades",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Units",
                table: "Grades");
        }
    }
}
