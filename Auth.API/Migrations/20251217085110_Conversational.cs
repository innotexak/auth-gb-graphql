using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class Conversational : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Authentication",
                keyColumn: "Id",
                keyValue: new Guid("83c893a6-c923-4293-88a3-8748db9939b8"));

            migrationBuilder.DeleteData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("c141db04-6f2a-461a-a688-2d5015632b3d"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("1fcf85c9-e45a-49fa-acb8-c147a4faa324"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Username" },
                values: new object[] { new Guid("1863464c-eab2-48e9-ac73-34540703c820"), "user@gmail.com", "password123", "user123" });

            migrationBuilder.InsertData(
                table: "Authentication",
                columns: new[] { "Id", "RefreshToken", "RefreshTokenExpiryTime", "Role", "UserId" },
                values: new object[] { new Guid("200d7668-9428-4165-a288-ec07690914d5"), "refreshToken", new DateTime(2025, 12, 24, 8, 51, 10, 423, DateTimeKind.Utc).AddTicks(1124), "User", new Guid("1863464c-eab2-48e9-ac73-34540703c820") });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Content", "Description", "Title", "UserId" },
                values: new object[] { new Guid("a9eda155-df12-4307-9970-73e90f7662df"), "This is the content", "First Post", "Hello World", new Guid("1863464c-eab2-48e9-ac73-34540703c820") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Authentication",
                keyColumn: "Id",
                keyValue: new Guid("200d7668-9428-4165-a288-ec07690914d5"));

            migrationBuilder.DeleteData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("a9eda155-df12-4307-9970-73e90f7662df"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("1863464c-eab2-48e9-ac73-34540703c820"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Username" },
                values: new object[] { new Guid("1fcf85c9-e45a-49fa-acb8-c147a4faa324"), "user@gmail.com", "password123", "user123" });

            migrationBuilder.InsertData(
                table: "Authentication",
                columns: new[] { "Id", "RefreshToken", "RefreshTokenExpiryTime", "Role", "UserId" },
                values: new object[] { new Guid("83c893a6-c923-4293-88a3-8748db9939b8"), "refreshToken", new DateTime(2025, 12, 19, 14, 22, 10, 61, DateTimeKind.Utc).AddTicks(1044), "User", new Guid("1fcf85c9-e45a-49fa-acb8-c147a4faa324") });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Content", "Description", "Title", "UserId" },
                values: new object[] { new Guid("c141db04-6f2a-461a-a688-2d5015632b3d"), "This is the content", "First Post", "Hello World", new Guid("1fcf85c9-e45a-49fa-acb8-c147a4faa324") });
        }
    }
}
