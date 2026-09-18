using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodRelief.Migrations
{
    public partial class AddApprovedQuantityToSosRequestItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovedQuantity",
                table: "sos_request_items",
                type: "int",
                nullable: true);

            // ข้อมูลเดิมที่ผ่านการรับงานแล้วถือว่าอนุมัติเต็มจำนวนเดิม
            migrationBuilder.Sql(
                "UPDATE `sos_request_items` i INNER JOIN `sos_requests` r ON r.`Id` = i.`SosRequestId` SET i.`ApprovedQuantity` = i.`Quantity` WHERE r.`Status` <> 'Pending';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedQuantity",
                table: "sos_request_items");
        }
    }
}
