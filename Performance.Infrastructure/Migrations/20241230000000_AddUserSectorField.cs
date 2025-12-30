using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Performance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSectorField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sector",
                table: "Users");
        }
    }
}
