using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FoodDelivery.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailTemplateToPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TemplateKey",
                table: "EmailTemplates",
                newName: "Key");

            migrationBuilder.RenameColumn(
                name: "BodyContent",
                table: "EmailTemplates",
                newName: "Body");

            migrationBuilder.RenameIndex(
                name: "IX_EmailTemplates_TemplateKey",
                table: "EmailTemplates",
                newName: "IX_EmailTemplates_Key");

            migrationBuilder.CreateTable(
                name: "EmailSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Host = table.Column<string>(type: "text", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    EnableSsl = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSettings", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id-001",
                column: "ConcurrencyStamp",
                value: "0d691f51-a74a-451e-86ea-b4f3bcb9c787");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "24cff299-f7be-46b2-86a2-e51bdd8c50af", "AQAAAAIAAYagAAAAEHXtA7+dptKtJ3nUR20Cmib+raxEQwcrw0pTl056GsBNbO1b9U8nn9YIjK4UlD0XpA==", "64a7bc55-552a-4e47-82e9-09fa49d0a68f" });

            migrationBuilder.InsertData(
                table: "EmailSettings",
                columns: new[] { "Id", "DisplayName", "Email", "EnableSsl", "Host", "Password", "Port" },
                values: new object[] { 1, "Food Delivery Support", "admin@example.com", true, "smtp.gmail.com", "change_me", 587 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailSettings");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "EmailTemplates",
                newName: "TemplateKey");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "EmailTemplates",
                newName: "BodyContent");

            migrationBuilder.RenameIndex(
                name: "IX_EmailTemplates_Key",
                table: "EmailTemplates",
                newName: "IX_EmailTemplates_TemplateKey");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id-001",
                column: "ConcurrencyStamp",
                value: "8a789e07-041f-4da9-b7ad-7ffec7c5ff9b");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "de3c3846-2535-49f2-b89b-fa3177ff0e80", "AQAAAAIAAYagAAAAENCQ6rYzCJFG/NXR7U/w7fsjbw30JGDxtnmgbgZEUwDnZC8TCvaGpQqr7cKKhVXUug==", "e6b2d726-eb9c-433b-8df9-9e50da8f6330" });
        }
    }
}
