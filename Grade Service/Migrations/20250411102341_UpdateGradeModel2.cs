using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grade_Service.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGradeModel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfId",
                table: "Grades",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfId",
                table: "Grades");
        }
    }
}
