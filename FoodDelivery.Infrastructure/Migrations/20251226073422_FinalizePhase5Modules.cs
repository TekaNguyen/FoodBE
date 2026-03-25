using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDelivery.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinalizePhase5Modules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id-001",
                column: "ConcurrencyStamp",
                value: "731bc209-deb3-4170-b327-190dcf84e3c7");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f5e170d4-1a93-4258-bdc3-2e075669c041", "AQAAAAIAAYagAAAAED1+UlDXbX35/ACBChT69j/9EqBclp6PxhgMq2jkbru8me+nmcIfOgRv8rye4OwdCg==", "589d6cd5-3dba-4e9c-a838-1a8594dfa7b4" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
