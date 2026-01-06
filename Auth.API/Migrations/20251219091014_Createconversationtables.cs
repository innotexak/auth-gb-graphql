using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class Createconversationtables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConversationParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationParticipants_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConversationParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DirectMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectMessages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DirectMessages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "IX_ConversationParticipants_ConversationId_UserId",
                table: "ConversationParticipants",
                columns: new[] { "ConversationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_UserId",
                table: "ConversationParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Type",
                table: "Conversations",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DirectMessages_ConversationId",
                table: "DirectMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectMessages_SenderId",
                table: "DirectMessages",
                column: "SenderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationParticipants");

            migrationBuilder.DropTable(
                name: "DirectMessages");

            migrationBuilder.DropTable(
                name: "Conversations");

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
    }
}
