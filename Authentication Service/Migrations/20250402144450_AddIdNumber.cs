using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authentication_Service.Migrations
{
    /// <inheritdoc />
    public partial class AddIdNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "idNumber",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "idNumber",
                table: "AspNetUsers");
        }
    }
}
