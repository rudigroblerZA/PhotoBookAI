using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoBook.AILayoutService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LayoutCache",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CacheKey = table.Column<string>(type: "text", nullable: false),
                    SuggestionsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LayoutCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhotoAnalysisCache",
                columns: table => new
                {
                    PhotoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnalysisJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoAnalysisCache", x => x.PhotoId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LayoutCache_CacheKey",
                table: "LayoutCache",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LayoutCache_CreatedAt",
                table: "LayoutCache",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutCache_ExpiresAt",
                table: "LayoutCache",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoAnalysisCache_CreatedAt",
                table: "PhotoAnalysisCache",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LayoutCache");

            migrationBuilder.DropTable(
                name: "PhotoAnalysisCache");
        }
    }
}
