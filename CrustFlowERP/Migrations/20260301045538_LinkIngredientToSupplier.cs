using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrustFlowERP.Migrations
{
    /// <inheritdoc />
    public partial class LinkIngredientToSupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreferredSupplierId",
                table: "Ingredients",
                type: "int",
                nullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_Suppliers_PreferredSupplierId",
                table: "Ingredients");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_PreferredSupplierId",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "PreferredSupplierId",
                table: "Ingredients");
        }
    }
}
