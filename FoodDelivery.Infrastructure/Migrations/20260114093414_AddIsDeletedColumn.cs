using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDelivery.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ChatMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id-001",
                column: "ConcurrencyStamp",
                value: "e3e0182d-e439-4b40-acdb-6d630e07d4ac");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "91a61ce9-1b24-4be6-be67-ab33f24be1fd", "AQAAAAIAAYagAAAAEL28ks6OFi6hj21dDhA4Pfpm3CDuzAPkEuwc6vKupSDC1y0luekPz/Un0/X6/J6w7w==", "9f946504-5c83-4640-842a-3ff96dd0c18d" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ChatMessages");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id-001",
                column: "ConcurrencyStamp",
                value: "88f15edc-0fe8-491c-abff-0c1bac508b95");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d9fdd235-384a-4dea-a821-a63a9dc9ee5d", "AQAAAAIAAYagAAAAEKWja19tOpSTcJa9AYSp/Cyc5s4vPpmx632QgyBlUvjx+G9Hd0l2yXSprmdHwjNkPg==", "7afd14d8-7488-407a-9811-7cc0bbedecc9" });
        }
    }
}
