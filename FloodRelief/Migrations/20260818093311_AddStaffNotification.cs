using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodRelief.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "notifications",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "StaffId",
                table: "notifications",
                type: "varchar(5)",
                maxLength: 5,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_StaffId_IsRead_CreatedAt",
                table: "notifications",
                columns: new[] { "StaffId", "IsRead", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_staffs_StaffId",
                table: "notifications",
                column: "StaffId",
                principalTable: "staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notifications_staffs_StaffId",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_StaffId_IsRead_CreatedAt",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "notifications");

            migrationBuilder.UpdateData(
                table: "notifications",
                keyColumn: "UserId",
                keyValue: null,
                column: "UserId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "notifications",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
