using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrustFlowERP.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PreferredSupplierId",
                table: "Inventories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_PreferredSupplierId",
                table: "Inventories",
                column: "PreferredSupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Suppliers_PreferredSupplierId",
                table: "Inventories",
                column: "PreferredSupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Suppliers_PreferredSupplierId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_PreferredSupplierId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "PreferredSupplierId",
                table: "Inventories");
        }
    }
}
