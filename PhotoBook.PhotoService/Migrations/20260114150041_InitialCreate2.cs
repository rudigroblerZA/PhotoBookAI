using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoBook.PhotoService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Photos_DateTaken",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_UploadedAt",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "DateTaken",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "Photos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateTaken",
                table: "Photos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "Photos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Photos_DateTaken",
                table: "Photos",
                column: "DateTaken");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_UploadedAt",
                table: "Photos",
                column: "UploadedAt");
        }
    }
}
