using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medicare_Connect.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Consultations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoctorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    WhenUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Consultations");
        }
    }
}
