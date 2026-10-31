using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ZurichClaims.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolicyNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    InsuredName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EstimatedAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CurrentStatus = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClaimStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClaimId = table.Column<int>(type: "INTEGER", nullable: false),
                    PreviousStatus = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    NewStatus = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    ChangeReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimStatusHistories_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Claims",
                columns: new[] { "Id", "ClaimType", "CreatedAt", "CurrentStatus", "EstimatedAmount", "InsuredName", "PolicyNumber", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Colisión vehicular", new DateTime(2026, 1, 10, 9, 0, 0, 0, DateTimeKind.Utc), "abierto", 18500.00m, "Ana Gómez", "POL-2024-001", null },
                    { 2, "Robo de autopartes", new DateTime(2026, 1, 12, 14, 30, 0, 0, DateTimeKind.Utc), "en revisión", 12000.00m, "Carlos Méndez", "POL-2024-002", new DateTime(2026, 1, 13, 10, 15, 0, 0, DateTimeKind.Utc) },
                    { 3, "Daños por inundación", new DateTime(2026, 1, 5, 11, 0, 0, 0, DateTimeKind.Utc), "cerrado", 45000.00m, "Mariana Torres", "POL-2024-003", new DateTime(2026, 1, 20, 16, 45, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "ClaimStatusHistories",
                columns: new[] { "Id", "ChangeReason", "ChangedAt", "ClaimId", "NewStatus", "PreviousStatus" },
                values: new object[,]
                {
                    { 1, "Apertura y registro inicial del siniestro", new DateTime(2026, 1, 10, 9, 0, 0, 0, DateTimeKind.Utc), 1, "abierto", null },
                    { 2, "Apertura y registro inicial del siniestro", new DateTime(2026, 1, 12, 14, 30, 0, 0, DateTimeKind.Utc), 2, "abierto", null },
                    { 3, "Asignación a perito técnico para inspección física", new DateTime(2026, 1, 13, 10, 15, 0, 0, DateTimeKind.Utc), 2, "en revisión", "abierto" },
                    { 4, "Apertura y registro inicial del siniestro", new DateTime(2026, 1, 5, 11, 0, 0, 0, DateTimeKind.Utc), 3, "abierto", null },
                    { 5, "Revisión de cobertura de póliza y deducible", new DateTime(2026, 1, 8, 15, 20, 0, 0, DateTimeKind.Utc), 3, "en revisión", "abierto" },
                    { 6, "Indemnización liquidada y finiquitada por el asegurado", new DateTime(2026, 1, 20, 16, 45, 0, 0, DateTimeKind.Utc), 3, "cerrado", "en revisión" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_PolicyNumber",
                table: "Claims",
                column: "PolicyNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimStatusHistories_ClaimId",
                table: "ClaimStatusHistories",
                column: "ClaimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimStatusHistories");

            migrationBuilder.DropTable(
                name: "Claims");
        }
    }
}
