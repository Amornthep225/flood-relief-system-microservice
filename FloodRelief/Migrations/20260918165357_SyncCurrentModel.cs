using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodRelief.Migrations
{
    /// <inheritdoc />
    public partial class SyncCurrentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeathCount",
                table: "sos_requests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReceiveMethod",
                table: "sos_requests",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "sos_requests",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ApprovedQuantity",
                table: "sos_request_items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDonationOpen",
                table: "relief_items",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaximumRequestQuantity",
                table: "relief_items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaximumQuantity",
                table: "center_inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeathCount",
                table: "sos_requests");

            migrationBuilder.DropColumn(
                name: "ReceiveMethod",
                table: "sos_requests");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "sos_requests");

            migrationBuilder.DropColumn(
                name: "ApprovedQuantity",
                table: "sos_request_items");

            migrationBuilder.DropColumn(
                name: "IsDonationOpen",
                table: "relief_items");

            migrationBuilder.DropColumn(
                name: "MaximumRequestQuantity",
                table: "relief_items");

            migrationBuilder.DropColumn(
                name: "MaximumQuantity",
                table: "center_inventories");
        }
    }
}
