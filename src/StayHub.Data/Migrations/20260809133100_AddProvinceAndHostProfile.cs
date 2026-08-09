using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StayHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProvinceAndHostProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Properties",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "AspNetUsers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "AspNetUsers",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_IsActive_PricePerNight",
                table: "Properties",
                columns: new[] { "IsActive", "PricePerNight" });

            migrationBuilder.CreateIndex(
                name: "IX_Properties_Province",
                table: "Properties",
                column: "Province");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Properties_IsActive_PricePerNight",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_Province",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AspNetUsers");
        }
    }
}
