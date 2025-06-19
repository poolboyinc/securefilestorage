using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureFileStorage.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuditLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileAuditLogs_SecureFiles_FileId",
                table: "FileAuditLogs");

            migrationBuilder.CreateTable(
                name: "UserAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAuditLogs_UserId",
                table: "UserAuditLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileAuditLogs_SecureFiles_FileId",
                table: "FileAuditLogs",
                column: "FileId",
                principalTable: "SecureFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileAuditLogs_SecureFiles_FileId",
                table: "FileAuditLogs");

            migrationBuilder.DropTable(
                name: "UserAuditLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_FileAuditLogs_SecureFiles_FileId",
                table: "FileAuditLogs",
                column: "FileId",
                principalTable: "SecureFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
