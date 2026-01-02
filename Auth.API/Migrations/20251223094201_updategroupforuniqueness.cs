using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class updategroupforuniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Groups",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Username" },
                values: new object[] { new Guid("c691d7ac-e71f-4311-8f9b-a8b240c17377"), "user@gmail.com", "password123", "user123" });

            migrationBuilder.InsertData(
                table: "Authentication",
                columns: new[] { "Id", "RefreshToken", "RefreshTokenExpiryTime", "Role", "UserId" },
                values: new object[] { new Guid("fcf06151-e5eb-408b-bd1e-8959a71aaa63"), "refreshToken", new DateTime(2025, 12, 30, 9, 42, 1, 368, DateTimeKind.Utc).AddTicks(9367), "User", new Guid("c691d7ac-e71f-4311-8f9b-a8b240c17377") });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Content", "Description", "Title", "UserId" },
                values: new object[] { new Guid("11b2773f-ee76-40d6-8397-85540a1474f1"), "This is the content", "First Post", "Hello World", new Guid("c691d7ac-e71f-4311-8f9b-a8b240c17377") });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_UserId_Title",
                table: "Groups",
                columns: new[] { "UserId", "Title" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Groups_UserId_Title",
                table: "Groups");

            migrationBuilder.DeleteData(
                table: "Authentication",
                keyColumn: "Id",
                keyValue: new Guid("fcf06151-e5eb-408b-bd1e-8959a71aaa63"));

            migrationBuilder.DeleteData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("11b2773f-ee76-40d6-8397-85540a1474f1"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c691d7ac-e71f-4311-8f9b-a8b240c17377"));

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Groups",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

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
        }
    }
}
