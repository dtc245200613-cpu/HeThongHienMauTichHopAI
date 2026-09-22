using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongHienMauTichHopAI.Migrations
{
    /// <inheritdoc />
    public partial class SyncDonationRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DonationRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VolunteerName = table.Column<string>(type: "TEXT", nullable: false),
                    BloodType = table.Column<string>(type: "TEXT", nullable: false),
                    CampaignId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonationRegistrations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DonationRegistrations");
        }
    }
}
