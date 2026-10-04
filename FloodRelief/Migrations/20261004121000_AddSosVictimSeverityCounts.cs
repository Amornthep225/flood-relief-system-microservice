using FloodRelief.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodRelief.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261004121000_AddSosVictimSeverityCounts")]
    public partial class AddSosVictimSeverityCounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sos_victim_severity_counts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SosRequestId = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Severity = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChildCount = table.Column<int>(type: "int", nullable: false),
                    AdultCount = table.Column<int>(type: "int", nullable: false),
                    ElderlyCount = table.Column<int>(type: "int", nullable: false),
                    DisabledCount = table.Column<int>(type: "int", nullable: false),
                    PatientCount = table.Column<int>(type: "int", nullable: false),
                    DeathCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sos_victim_severity_counts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sos_victim_severity_counts_sos_requests_SosRequestId",
                        column: x => x.SosRequestId,
                        principalTable: "sos_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_sos_victim_severity_counts_SosRequestId_Severity",
                table: "sos_victim_severity_counts",
                columns: new[] { "SosRequestId", "Severity" },
                unique: true);

            // ย้ายข้อมูล SOS เดิมเข้ารูปแบบใหม่โดยไม่ลบข้อมูลเก่า
            migrationBuilder.Sql(@"
                INSERT INTO sos_victim_severity_counts
                    (SosRequestId, Severity, ChildCount, AdultCount, ElderlyCount, DisabledCount, PatientCount, DeathCount)
                SELECT
                    Id,
                    COALESCE(NULLIF(Severity, ''), 'Mild'),
                    ChildCount,
                    GREATEST(VictimCount - ChildCount - ElderlyCount - DisabledCount - PatientCount - DeathCount, 0),
                    ElderlyCount,
                    DisabledCount,
                    PatientCount,
                    DeathCount
                FROM sos_requests
                WHERE RequestType = 'Emergency'
                  AND (VictimCount > 0 OR ChildCount > 0 OR ElderlyCount > 0 OR DisabledCount > 0 OR PatientCount > 0 OR DeathCount > 0);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sos_victim_severity_counts");
        }
    }
}
