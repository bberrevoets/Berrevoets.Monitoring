using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Berrevoets.Monitoring.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddIconToQuickLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "QuickLinks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "QuickLinks");
        }
    }
}
