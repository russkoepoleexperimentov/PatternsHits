using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Web.Migrations
{
    /// <inheritdoc />
    public partial class Transaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_FromAccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_FromAccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "FromAccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "Transactions",
                newName: "ResolvedAt");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Transactions",
                type: "integer",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetType",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SourceId",
                table: "Transactions",
                column: "SourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Accounts_AccountId",
                table: "Transactions",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_AccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_SourceId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TargetType",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "ResolvedAt",
                table: "Transactions",
                newName: "CompletedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Transactions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "FromAccountId",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Transactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_FromAccountId",
                table: "Transactions",
                column: "FromAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Accounts_FromAccountId",
                table: "Transactions",
                column: "FromAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
