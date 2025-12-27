using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FoodDelivery.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase5AnalyticsAndReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsletterSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UnsubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterSubscriptions", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsletterSubscriptions");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "admin-role-id-001",
                column: "ConcurrencyStamp",
                value: "1eaaf649-67fa-4056-9012-435f623e8561");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-001",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e741b7ba-c111-4bbb-ae4c-dc1f892f4578", "AQAAAAIAAYagAAAAEB5Yo+1abS3fcraMihr76OIhjNgSdScRCDP/3378P7FCX3EzDhn+1UWwdkJqZY1Pow==", "76ee8832-8502-4f02-a61b-5e8e60b105fc" });
        }
    }
}
