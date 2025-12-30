using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Performance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ManagerNotes",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerNotes",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManagerNotes",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ManagerNotes",
                table: "Projects");
        }
    }
}
