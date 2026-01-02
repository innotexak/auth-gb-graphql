using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class creategrouptable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Username" },
                values: new object[] { new Guid("973b8352-3171-41e5-944e-a8b01f0a06fe"), "user@gmail.com", "password123", "user123" });

            migrationBuilder.InsertData(
                table: "Authentication",
                columns: new[] { "Id", "RefreshToken", "RefreshTokenExpiryTime", "Role", "UserId" },
                values: new object[] { new Guid("cc67e33c-39f4-40f8-b750-c26b2f0ede1d"), "refreshToken", new DateTime(2025, 12, 30, 8, 8, 25, 895, DateTimeKind.Utc).AddTicks(2749), "User", new Guid("973b8352-3171-41e5-944e-a8b01f0a06fe") });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Content", "Description", "Title", "UserId" },
                values: new object[] { new Guid("d4b024b8-f84f-47f3-a39f-e1acae235f9d"), "This is the content", "First Post", "Hello World", new Guid("973b8352-3171-41e5-944e-a8b01f0a06fe") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DeleteData(
                table: "Authentication",
                keyColumn: "Id",
                keyValue: new Guid("cc67e33c-39f4-40f8-b750-c26b2f0ede1d"));

            migrationBuilder.DeleteData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("d4b024b8-f84f-47f3-a39f-e1acae235f9d"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("973b8352-3171-41e5-944e-a8b01f0a06fe"));

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
        }
    }
}
