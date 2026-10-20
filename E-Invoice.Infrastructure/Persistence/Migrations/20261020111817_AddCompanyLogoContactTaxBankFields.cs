using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EInvoice.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyLogoContactTaxBankFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccountHolderName",
                table: "Companies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BankCountryId",
                table: "Companies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankCurrency",
                table: "Companies",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Companies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPersonName",
                table: "Companies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPersonPhone",
                table: "Companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "Companies",
                type: "character varying(34)",
                maxLength: 34,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "LogoContent",
                table: "Companies",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoContentType",
                table: "Companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PeppolParticipantId",
                table: "Companies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SwiftCode",
                table: "Companies",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tin",
                table: "Companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Trn",
                table: "Companies",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_BankCountryId",
                table: "Companies",
                column: "BankCountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Countries_BankCountryId",
                table: "Companies",
                column: "BankCountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Countries_BankCountryId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_BankCountryId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "BankAccountHolderName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "BankCountryId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "BankCurrency",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ContactPersonName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ContactPersonPhone",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LogoContent",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LogoContentType",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PeppolParticipantId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SwiftCode",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Tin",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Trn",
                table: "Companies");
        }
    }
}
