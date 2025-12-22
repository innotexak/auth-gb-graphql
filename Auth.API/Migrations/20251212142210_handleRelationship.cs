using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class handleRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Authentication",
                keyColumn: "Id",
                keyValue: new Guid("e90aa382-80bb-4bd0-bad4-e1186479dc80"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("4fa1dfc5-33c3-4383-bb93-5d677f145fd9"));

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId",
                table: "Posts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DeleteData(
                table: "Authentication",
                keyColumn: "Id",
                keyValue: new Guid("83c893a6-c923-4293-88a3-8748db9939b8"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("1fcf85c9-e45a-49fa-acb8-c147a4faa324"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Username" },
                values: new object[] { new Guid("4fa1dfc5-33c3-4383-bb93-5d677f145fd9"), "user@gmail.com", "password123", "user123" });

            migrationBuilder.InsertData(
                table: "Authentication",
                columns: new[] { "Id", "RefreshToken", "RefreshTokenExpiryTime", "Role", "UserId" },
                values: new object[] { new Guid("e90aa382-80bb-4bd0-bad4-e1186479dc80"), "refreshToken", new DateTime(2025, 12, 19, 11, 12, 40, 100, DateTimeKind.Utc).AddTicks(5925), "User", new Guid("4fa1dfc5-33c3-4383-bb93-5d677f145fd9") });
        }
    }
}
