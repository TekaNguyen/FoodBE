using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDelivery.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedToUserId",
                table: "Conversations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedToUserName",
                table: "Conversations",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id-001",
                column: "ConcurrencyStamp",
                value: "aa3910b9-7ecf-4486-b372-75ea3da04400");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "06d4bbaa-2f9a-446d-8ca8-1fcc09ab3ec3", "AQAAAAIAAYagAAAAEG+lufD7KykDujYJgu9f3cz2++n4QRoYLJ1n9xtXc2mJrFaGmsFUc15Yk+WiFcdCcA==", "b3b39b00-5748-4b77-bfc8-156060d391bd" });

            migrationBuilder.UpdateData(
                table: "Conversations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AssignedToUserId", "AssignedToUserName" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_AssignedToUserId",
                table: "Conversations",
                column: "AssignedToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_AspNetUsers_AssignedToUserId",
                table: "Conversations",
                column: "AssignedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_AspNetUsers_AssignedToUserId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_AssignedToUserId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "AssignedToUserName",
                table: "Conversations");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id-001",
                column: "ConcurrencyStamp",
                value: "86fe8302-146f-48bd-bec1-8a6fa47b8950");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c8caab4a-4c74-4f54-8cf8-6ea590e33bad", "AQAAAAIAAYagAAAAENOBLGjqloIx314KvxXnJNkK2gVcIheIADsY+CRdEsy2XpbmUOf4phmMwyr5pHaPOg==", "ab0ba0d3-b384-458b-b0f8-d2bd7a6688a9" });
        }
    }
}
