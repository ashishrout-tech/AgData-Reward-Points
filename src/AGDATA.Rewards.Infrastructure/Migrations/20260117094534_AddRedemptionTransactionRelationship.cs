using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRedemptionTransactionRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RedemptionId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeductionTransactionId",
                table: "Redemptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RefundTransactionId",
                table: "Redemptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RedemptionId",
                table: "Transactions",
                column: "RedemptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Redemptions_DeductionTransactionId",
                table: "Redemptions",
                column: "DeductionTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Redemptions_RefundTransactionId",
                table: "Redemptions",
                column: "RefundTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Redemption_DeductionTransaction",
                table: "Redemptions",
                column: "DeductionTransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Redemption_RefundTransaction",
                table: "Redemptions",
                column: "RefundTransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Redemption",
                table: "Transactions",
                column: "RedemptionId",
                principalTable: "Redemptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Redemption_DeductionTransaction",
                table: "Redemptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Redemption_RefundTransaction",
                table: "Redemptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Redemption",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_RedemptionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Redemptions_DeductionTransactionId",
                table: "Redemptions");

            migrationBuilder.DropIndex(
                name: "IX_Redemptions_RefundTransactionId",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "RedemptionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DeductionTransactionId",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "RefundTransactionId",
                table: "Redemptions");
        }
    }
}
