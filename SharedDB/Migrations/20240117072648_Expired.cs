using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedDB.Migrations
{
    /// <inheritdoc />
    public partial class Expired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Token",
                newName: "Expired");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Expired",
                table: "Token",
                newName: "Created");
        }
    }
}
