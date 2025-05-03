using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoggingService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clinics",
                columns: table => new
                {
                    ClinicId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClinicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinics", x => x.ClinicId);
                });

            migrationBuilder.CreateTable(
                name: "Digests",
                columns: table => new
                {
                    DigestId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorCount = table.Column<int>(type: "int", nullable: false),
                    OutstandingCount = table.Column<int>(type: "int", nullable: false),
                    SentStatus = table.Column<bool>(type: "bit", nullable: false),
                    Recipients = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Digests", x => x.DigestId);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MessageId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClinicId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OriginalFhirContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConvertedHl7Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResendCount = table.Column<int>(type: "int", nullable: false),
                    RetentionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageId);
                });

            migrationBuilder.CreateTable(
                name: "Transmissions",
                columns: table => new
                {
                    TransmissionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MessageId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AcknowledgmentType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ErrorDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transmissions", x => x.TransmissionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ClinicId",
                table: "Messages",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_PatientId",
                table: "Messages",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Status",
                table: "Messages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Transmissions_MessageId",
                table: "Transmissions",
                column: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clinics");

            migrationBuilder.DropTable(
                name: "Digests");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Transmissions");
        }
    }
}
