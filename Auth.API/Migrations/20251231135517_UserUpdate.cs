using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class UserUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Preferences_EmailNotification",
                table: "Users",
                type: "bit",
                maxLength: 10,
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Preferences_ProfileVisibility",
                table: "Users",
                type: "int",
                maxLength: 10,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "Bio", "Email", "FirstName", "LastName", "Password", "Username" },
                values: new object[] { new Guid("76953481-9f77-4a6a-b867-da9e1f93a486"), "default-avatar.png", "Software Developer", "user@gmail.com", "", "", "password123", "user123" });

            migrationBuilder.InsertData(
                table: "Authentication",
                columns: new[] { "Id", "RefreshToken", "RefreshTokenExpiryTime", "Role", "UserId" },
                values: new object[] { new Guid("ad2b0a41-ca0f-445a-b36e-0dcc1f122b78"), "refreshToken", new DateTime(2026, 1, 7, 13, 55, 17, 409, DateTimeKind.Utc).AddTicks(3038), "User", new Guid("76953481-9f77-4a6a-b867-da9e1f93a486") });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Content", "Description", "Title", "UserId" },
                values: new object[] { new Guid("b83957bf-91eb-49bf-9fec-a787dba86157"), "This is the content", "First Post", "Hello World", new Guid("76953481-9f77-4a6a-b867-da9e1f93a486") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Authentication",
                keyColumn: "Id",
                keyValue: new Guid("ad2b0a41-ca0f-445a-b36e-0dcc1f122b78"));

            migrationBuilder.DeleteData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("b83957bf-91eb-49bf-9fec-a787dba86157"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("76953481-9f77-4a6a-b867-da9e1f93a486"));

            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Preferences_EmailNotification",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Preferences_ProfileVisibility",
                table: "Users");

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
    }
}
