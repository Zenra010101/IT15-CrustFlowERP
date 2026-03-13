using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrustFlowERP.Migrations
{
    /// <inheritdoc />
    public partial class AddNextDueDateToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextDueDate",
                table: "Tenants",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextDueDate",
                table: "Tenants");
        }
    }
}
