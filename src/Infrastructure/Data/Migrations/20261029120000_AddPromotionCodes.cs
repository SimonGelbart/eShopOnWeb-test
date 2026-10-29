using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microsoft.eShopWeb.Infrastructure.Data.Migrations;

public partial class AddPromotionCodes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PromoCode",
            table: "Baskets",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "DiscountAmount",
            table: "Orders",
            type: "decimal(18,2)",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<string>(
            name: "PromoCode",
            table: "Orders",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PromoCode",
            table: "Baskets");

        migrationBuilder.DropColumn(
            name: "DiscountAmount",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "PromoCode",
            table: "Orders");
    }
}
