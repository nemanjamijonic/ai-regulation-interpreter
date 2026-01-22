using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Common.Migrations
{
    /// <inheritdoc />
    public partial class RemoveContentAndSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentSections");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "DocumentVersions");

            migrationBuilder.AddColumn<string>(
                name: "IndexError",
                table: "DocumentVersions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndexStatus",
                table: "DocumentVersions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "IndexedAt",
                table: "DocumentVersions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_IndexStatus",
                table: "DocumentVersions",
                column: "IndexStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DocumentVersions_IndexStatus",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "IndexError",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "IndexStatus",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "IndexedAt",
                table: "DocumentVersions");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "DocumentVersions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DocumentSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    ParentSectionId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Identifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentSections_DocumentSections_ParentSectionId",
                        column: x => x.ParentSectionId,
                        principalTable: "DocumentSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentSections_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSections_DocumentId_OrderIndex",
                table: "DocumentSections",
                columns: new[] { "DocumentId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSections_ParentSectionId",
                table: "DocumentSections",
                column: "ParentSectionId");
        }
    }
}
