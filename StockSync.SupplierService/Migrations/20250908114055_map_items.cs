using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockSync.SupplierService.Migrations
{
    /// <inheritdoc />
    public partial class map_items : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Items",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Items",
                table: "Suppliers");
        }
    }
}
