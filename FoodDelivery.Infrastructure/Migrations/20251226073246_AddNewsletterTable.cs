using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDelivery.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id-001",
                column: "ConcurrencyStamp",
                value: "a8d3ceaf-d6d2-4950-8484-6dab702628ab");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3251e6a7-8332-408a-aaa6-5653873fe296", "AQAAAAIAAYagAAAAECnZiHQKynZjFh6kkZ4gkkaUCtI/5GQ0xDyILJCEbf3gohawKJ9nu/JI+TFCGwgZTQ==", "0f47ad59-50d0-413f-b1db-ce1f25d4d77f" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id-001",
                column: "ConcurrencyStamp",
                value: "7e065c47-94aa-41c4-9946-d3f187b50898");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d4c16d64-7057-451c-8ca3-0d986aba0a85", "AQAAAAIAAYagAAAAENucnyB9vRbRjoUrJiPhBsZDQcpDv77gmob/WIBrUBStL/kwOKPkLQWpbDaH005pVQ==", "8672e1ea-ff7a-48a3-a47b-1c0864628aeb" });
        }
    }
}
