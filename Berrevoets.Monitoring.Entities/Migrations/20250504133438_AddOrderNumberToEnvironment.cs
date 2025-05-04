using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Berrevoets.Monitoring.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderNumberToEnvironment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderNumber",
                table: "MonitoredEnvironments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "MonitoredEnvironments");
        }
    }
}
