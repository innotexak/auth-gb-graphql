using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class updateconversationtableindex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conversations_Type",
                table: "Conversations");

            migrationBuilder.DeleteData(
                table: "Authentication",
                keyColumn: "Id",
                keyValue: new Guid("a759dce4-1c1c-49fd-a245-80858fba3c62"));

            migrationBuilder.DeleteData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("88671410-280b-41a7-a8dd-b2c800af6132"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("18e147e0-fa41-4579-b554-a4e1331ea1fc"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Username" },
                values: new object[] { new Guid("deba4264-5482-4ca5-a74d-d02ecf5d19af"), "user@gmail.com", "password123", "user123" });

            migrationBuilder.InsertData(
                table: "Authentication",
                columns: new[] { "Id", "RefreshToken", "RefreshTokenExpiryTime", "Role", "UserId" },
                values: new object[] { new Guid("39ff6063-0989-49bb-a1e9-c034f840828d"), "refreshToken", new DateTime(2025, 12, 26, 9, 59, 13, 681, DateTimeKind.Utc).AddTicks(6283), "User", new Guid("deba4264-5482-4ca5-a74d-d02ecf5d19af") });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Content", "Description", "Title", "UserId" },
                values: new object[] { new Guid("9e618ef3-de15-4912-be10-ee4c81b6bcbc"), "This is the content", "First Post", "Hello World", new Guid("deba4264-5482-4ca5-a74d-d02ecf5d19af") });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Type",
                table: "Conversations",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conversations_Type",
                table: "Conversations");

            migrationBuilder.DeleteData(
                table: "Authentication",
                keyColumn: "Id",
                keyValue: new Guid("39ff6063-0989-49bb-a1e9-c034f840828d"));

            migrationBuilder.DeleteData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("9e618ef3-de15-4912-be10-ee4c81b6bcbc"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("deba4264-5482-4ca5-a74d-d02ecf5d19af"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Username" },
                values: new object[] { new Guid("18e147e0-fa41-4579-b554-a4e1331ea1fc"), "user@gmail.com", "password123", "user123" });

            migrationBuilder.InsertData(
                table: "Authentication",
                columns: new[] { "Id", "RefreshToken", "RefreshTokenExpiryTime", "Role", "UserId" },
                values: new object[] { new Guid("a759dce4-1c1c-49fd-a245-80858fba3c62"), "refreshToken", new DateTime(2025, 12, 26, 9, 10, 14, 767, DateTimeKind.Utc).AddTicks(86), "User", new Guid("18e147e0-fa41-4579-b554-a4e1331ea1fc") });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Content", "Description", "Title", "UserId" },
                values: new object[] { new Guid("88671410-280b-41a7-a8dd-b2c800af6132"), "This is the content", "First Post", "Hello World", new Guid("18e147e0-fa41-4579-b554-a4e1331ea1fc") });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Type",
                table: "Conversations",
                column: "Type",
                unique: true);
        }
    }
}
