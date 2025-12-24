using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class NewUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<Guid>(
                name: "ConversationId",
                table: "Groups",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Conversations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "ConversationParticipants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "ConversationParticipants",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ConversationId",
                table: "Groups",
                column: "ConversationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Conversations_ConversationId",
                table: "Groups",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Conversations_ConversationId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_ConversationId",
                table: "Groups");

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

            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ConversationParticipants");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ConversationParticipants");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Conversations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

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
        }
    }
}
