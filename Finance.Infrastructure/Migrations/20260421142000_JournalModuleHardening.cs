using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class JournalModuleHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "accounting",
                table: "JournalEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "accounting",
                table: "JournalEntries",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "accounting",
                table: "JournalEntries",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                schema: "accounting",
                table: "JournalEntries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "accounting",
                table: "JournalEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE accounting."JournalEntries"
                SET "CreatedAt" = COALESCE("CreatedAt", CURRENT_TIMESTAMP),
                    "CreatedBy" = COALESCE(NULLIF(BTRIM("CreatedBy"), ''), 'legacy-migration'),
                    "Description" = COALESCE(NULLIF(BTRIM("Description"), ''), 'Legacy journal entry'),
                    "Status" = COALESCE("Status", 1);
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "accounting",
                table: "JournalEntries",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "accounting",
                table: "JournalEntries",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "accounting",
                table: "JournalEntries",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "accounting",
                table: "JournalEntries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_JournalEntryLines_NonNegativeAmounts",
                schema: "accounting",
                table: "JournalEntryLines",
                sql: "\"Debit\" >= 0 AND \"Credit\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_JournalEntryLines_SingleSidedAmount",
                schema: "accounting",
                table: "JournalEntryLines",
                sql: "(CASE WHEN \"Debit\" > 0 THEN 1 ELSE 0 END) + (CASE WHEN \"Credit\" > 0 THEN 1 ELSE 0 END) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_JournalEntries_Status",
                schema: "accounting",
                table: "JournalEntries",
                sql: "\"Status\" IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_JournalEntryLines_NonNegativeAmounts",
                schema: "accounting",
                table: "JournalEntryLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_JournalEntryLines_SingleSidedAmount",
                schema: "accounting",
                table: "JournalEntryLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_JournalEntries_Status",
                schema: "accounting",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "accounting",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "accounting",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "accounting",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                schema: "accounting",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "accounting",
                table: "JournalEntries");
        }
    }
}
