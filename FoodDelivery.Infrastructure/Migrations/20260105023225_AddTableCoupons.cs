using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDelivery.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTableCoupons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id-001",
                column: "ConcurrencyStamp",
                value: "5fbbb2a4-a604-481c-9946-0b726ac07266");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9c425638-94b1-459c-a5c5-ad6382dd2a8a", "AQAAAAIAAYagAAAAEKLoM135sr8eYVEALn0qfhbMktMS/rAd3hP6HkkdPcoVTk6LAkLvn8FT5JBY0i4PGA==", "1423eb0a-c17a-49fa-a754-661261feaf6a" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id-001",
                column: "ConcurrencyStamp",
                value: "67c80000-4868-4612-b75d-4808dfcfbbd1");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9fa66caa-0c4e-483a-850b-394c59efe670", "AQAAAAIAAYagAAAAEJ2VsKYwQFvotD9OCFthlCmLv/BOAAYmJ2lv9AjevpKsKOY3Twt0XaVLnhMQcVIqmA==", "59d842ce-d2fe-4727-9f94-06f503bc981e" });
        }
    }
}
