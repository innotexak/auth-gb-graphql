using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class updatedgroupdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Groups",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Username" },
                values: new object[] { new Guid("36c0e296-504a-4457-a2a3-8445696fdb85"), "user@gmail.com", "password123", "user123" });

            migrationBuilder.InsertData(
                table: "Authentication",
                columns: new[] { "Id", "RefreshToken", "RefreshTokenExpiryTime", "Role", "UserId" },
                values: new object[] { new Guid("4d74b3dd-55c1-4cec-a925-20c680836629"), "refreshToken", new DateTime(2025, 12, 30, 8, 29, 44, 246, DateTimeKind.Utc).AddTicks(7062), "User", new Guid("36c0e296-504a-4457-a2a3-8445696fdb85") });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Content", "Description", "Title", "UserId" },
                values: new object[] { new Guid("46f6c666-6325-4eb9-a01f-3760f8f53008"), "This is the content", "First Post", "Hello World", new Guid("36c0e296-504a-4457-a2a3-8445696fdb85") });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_UserId",
                table: "Groups",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Users_UserId",
                table: "Groups",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Users_UserId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_UserId",
                table: "Groups");

            migrationBuilder.DeleteData(
                table: "Authentication",
                keyColumn: "Id",
                keyValue: new Guid("4d74b3dd-55c1-4cec-a925-20c680836629"));

            migrationBuilder.DeleteData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("46f6c666-6325-4eb9-a01f-3760f8f53008"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("36c0e296-504a-4457-a2a3-8445696fdb85"));

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Groups");

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
    }
}
