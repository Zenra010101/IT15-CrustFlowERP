using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrustFlowERP.Migrations
{
    /// <inheritdoc />
    public partial class AddIsArchivedToInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_Suppliers_PreferredSupplierId",
                table: "Ingredients");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Suppliers_PreferredSupplierId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_PreferredSupplierId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_PreferredSupplierId",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "PreferredSupplierId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "PreferredSupplierId",
                table: "Ingredients");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Inventories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Inventories");

            migrationBuilder.AddColumn<int>(
                name: "PreferredSupplierId",
                table: "Inventories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredSupplierId",
                table: "Ingredients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_PreferredSupplierId",
                table: "Inventories",
                column: "PreferredSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_PreferredSupplierId",
                table: "Ingredients",
                column: "PreferredSupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_Suppliers_PreferredSupplierId",
                table: "Ingredients",
                column: "PreferredSupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Suppliers_PreferredSupplierId",
                table: "Inventories",
                column: "PreferredSupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }
    }
}
