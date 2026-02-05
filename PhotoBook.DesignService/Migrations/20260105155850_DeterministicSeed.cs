using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PhotoBook.DesignService.Migrations
{
    /// <inheritdoc />
    public partial class DeterministicSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Templates",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "IsActive", "LayoutJson", "Name", "PhotoSlots", "PreviewUrl" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Simple", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "One large photo per page", true, "{\"slots\":[{\"x\":0.1,\"y\":0.1,\"width\":0.8,\"height\":0.8}]}", "Single Hero", 1, "" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Simple", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Two photos side by side", true, "{\"slots\":[{\"x\":0.05,\"y\":0.1,\"width\":0.4,\"height\":0.8},{\"x\":0.55,\"y\":0.1,\"width\":0.4,\"height\":0.8}]}", "Side by Side", 2, "" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Grid", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Four photos in a grid", true, "{\"slots\":[{\"x\":0.05,\"y\":0.05,\"width\":0.4,\"height\":0.4},{\"x\":0.55,\"y\":0.05,\"width\":0.4,\"height\":0.4},{\"x\":0.05,\"y\":0.55,\"width\":0.4,\"height\":0.4},{\"x\":0.55,\"y\":0.55,\"width\":0.4,\"height\":0.4}]}", "Grid 4", 4, "" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Mixed", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "One large photo with two smaller ones", true, "{\"slots\":[{\"x\":0.05,\"y\":0.05,\"width\":0.6,\"height\":0.9},{\"x\":0.7,\"y\":0.05,\"width\":0.25,\"height\":0.4},{\"x\":0.7,\"y\":0.55,\"width\":0.25,\"height\":0.4}]}", "Feature + 2", 3, "" }
                });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "AccentColor", "BackgroundColor", "BorderColor", "BorderStyle", "BorderWidth", "Description", "FontFamily", "FontSize", "IsActive", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "#E74C3C", "#FFFFFF", "#2C3E50", "simple", 2, "Timeless elegance with clean lines", "Georgia", 14, true, "Classic", "#2C3E50", "#ECF0F1" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "#FFD700", "#FFFFFF", "#000000", "none", 0, "Bold and contemporary design", "Helvetica", 16, true, "Modern", "#000000", "#FFFFFF" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "#CD853F", "#FFF8DC", "#8B4513", "ornate", 3, "Nostalgic charm with warm tones", "Times New Roman", 13, true, "Vintage", "#8B4513", "#F5DEB3" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "#999999", "#FFFFFF", "#333333", "none", 0, "Clean, simple, and sophisticated", "Arial", 12, true, "Minimalist", "#333333", "#F9F9F9" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "#FF69B4", "#FFF0F5", "#C71585", "rounded", 2, "Soft pastels and elegant touches", "Brush Script MT", 15, true, "Romantic", "#C71585", "#FFE4E1" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Templates",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "Templates",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.DeleteData(
                table: "Templates",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"));

            migrationBuilder.DeleteData(
                table: "Templates",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"));

            migrationBuilder.DeleteData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Themes",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));
        }
    }
}
