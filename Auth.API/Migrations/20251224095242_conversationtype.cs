using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class conversationtype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Authentication",
                keyColumn: "Id",
                keyValue: new Guid("8fde9169-12b5-496f-a3a4-d914ccec7bc0"));

            migrationBuilder.DeleteData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("fbf5210f-d12d-4f90-a9eb-921f667af8b9"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5cd0616a-9824-4e8c-a801-b83209056fbf"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Username" },
                values: new object[] { new Guid("94d60db7-180e-4e9f-b955-fd118673d28f"), "user@gmail.com", "password123", "user123" });

            migrationBuilder.InsertData(
                table: "Authentication",
                columns: new[] { "Id", "RefreshToken", "RefreshTokenExpiryTime", "Role", "UserId" },
                values: new object[] { new Guid("bd74dfce-6028-4c28-9a30-8b23dd64841a"), "refreshToken", new DateTime(2025, 12, 31, 9, 52, 42, 784, DateTimeKind.Utc).AddTicks(1253), "User", new Guid("94d60db7-180e-4e9f-b955-fd118673d28f") });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Content", "Description", "Title", "UserId" },
                values: new object[] { new Guid("bbea6ce2-7a44-48be-93df-ba9e4a198ec6"), "This is the content", "First Post", "Hello World", new Guid("94d60db7-180e-4e9f-b955-fd118673d28f") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Authentication",
                keyColumn: "Id",
                keyValue: new Guid("bd74dfce-6028-4c28-9a30-8b23dd64841a"));

            migrationBuilder.DeleteData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("bbea6ce2-7a44-48be-93df-ba9e4a198ec6"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("94d60db7-180e-4e9f-b955-fd118673d28f"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Username" },
                values: new object[] { new Guid("5cd0616a-9824-4e8c-a801-b83209056fbf"), "user@gmail.com", "password123", "user123" });

            migrationBuilder.InsertData(
                table: "Authentication",
                columns: new[] { "Id", "RefreshToken", "RefreshTokenExpiryTime", "Role", "UserId" },
                values: new object[] { new Guid("8fde9169-12b5-496f-a3a4-d914ccec7bc0"), "refreshToken", new DateTime(2025, 12, 31, 9, 46, 17, 363, DateTimeKind.Utc).AddTicks(2118), "User", new Guid("5cd0616a-9824-4e8c-a801-b83209056fbf") });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Content", "Description", "Title", "UserId" },
                values: new object[] { new Guid("fbf5210f-d12d-4f90-a9eb-921f667af8b9"), "This is the content", "First Post", "Hello World", new Guid("5cd0616a-9824-4e8c-a801-b83209056fbf") });
        }
    }
}
