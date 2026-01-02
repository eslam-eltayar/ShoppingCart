using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShop.DataAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceAfterAndBefore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PriceAfterDiscount",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceBeforeDiscount",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceAfterDiscount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PriceBeforeDiscount",
                table: "Products");
        }
    }
}
