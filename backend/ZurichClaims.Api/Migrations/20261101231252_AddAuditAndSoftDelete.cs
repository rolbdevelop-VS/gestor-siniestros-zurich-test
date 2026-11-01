using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZurichClaims.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClaimStatusHistories_Claims_ClaimId",
                table: "ClaimStatusHistories");

            migrationBuilder.AddColumn<string>(
                name: "ChangedBy",
                table: "ClaimStatusHistories",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Claims",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Claims",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Claims",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Claims",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Claims",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ClaimStatusHistories",
                keyColumn: "Id",
                keyValue: 1,
                column: "ChangedBy",
                value: "0");

            migrationBuilder.UpdateData(
                table: "ClaimStatusHistories",
                keyColumn: "Id",
                keyValue: 2,
                column: "ChangedBy",
                value: "0");

            migrationBuilder.UpdateData(
                table: "ClaimStatusHistories",
                keyColumn: "Id",
                keyValue: 3,
                column: "ChangedBy",
                value: "0");

            migrationBuilder.UpdateData(
                table: "ClaimStatusHistories",
                keyColumn: "Id",
                keyValue: 4,
                column: "ChangedBy",
                value: "0");

            migrationBuilder.UpdateData(
                table: "ClaimStatusHistories",
                keyColumn: "Id",
                keyValue: 5,
                column: "ChangedBy",
                value: "0");

            migrationBuilder.UpdateData(
                table: "ClaimStatusHistories",
                keyColumn: "Id",
                keyValue: 6,
                column: "ChangedBy",
                value: "0");

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "DeletedAt", "DeletedBy", "UpdatedBy" },
                values: new object[] { "0", null, null, null });

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedBy", "DeletedAt", "DeletedBy", "UpdatedBy" },
                values: new object[] { "0", null, null, "0" });

            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedBy", "DeletedAt", "DeletedBy", "UpdatedBy" },
                values: new object[] { "0", null, null, "0" });

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimStatusHistories_Claims_ClaimId",
                table: "ClaimStatusHistories",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClaimStatusHistories_Claims_ClaimId",
                table: "ClaimStatusHistories");

            migrationBuilder.DropColumn(
                name: "ChangedBy",
                table: "ClaimStatusHistories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Claims");

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimStatusHistories_Claims_ClaimId",
                table: "ClaimStatusHistories",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
