using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP.Migrations
{
    /// <inheritdoc />
    public partial class DeleteDtUserRes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteDt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeleteDt",
                table: "Reservations");
        }
    }
}
