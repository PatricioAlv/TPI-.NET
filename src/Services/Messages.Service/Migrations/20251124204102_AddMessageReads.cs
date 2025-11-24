using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Messages.Service.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageReads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageReads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageReads_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageReads_MessageId",
                table: "MessageReads",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReads_MessageId_UserId",
                table: "MessageReads",
                columns: new[] { "MessageId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageReads_UserId",
                table: "MessageReads",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageReads");
        }
    }
}
