using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _240519P_AS_ASSN2.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddMaxPasswordAge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswordChanged",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPasswordChanged",
                table: "Users");
        }
    }
}
