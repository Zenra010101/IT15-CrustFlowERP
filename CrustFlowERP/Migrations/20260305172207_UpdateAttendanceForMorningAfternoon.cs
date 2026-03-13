using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrustFlowERP.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttendanceForMorningAfternoon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckIn",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "CheckOut",
                table: "Attendances",
                newName: "CheckOutMorning");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInAfternoon",
                table: "Attendances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInMorning",
                table: "Attendances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOutAfternoon",
                table: "Attendances",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInAfternoon",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckInMorning",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutAfternoon",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "CheckOutMorning",
                table: "Attendances",
                newName: "CheckOut");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckIn",
                table: "Attendances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
