using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Web.Migrations
{
    /// <inheritdoc />
    public partial class masterAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMaster",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMaster",
                table: "Accounts");
        }
    }
}
