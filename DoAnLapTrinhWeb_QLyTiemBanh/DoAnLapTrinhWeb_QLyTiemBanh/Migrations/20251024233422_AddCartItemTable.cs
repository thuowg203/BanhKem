using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnLapTrinhWeb_QLyTiemBanh.Migrations
{
    /// <inheritdoc />
    public partial class AddCartItemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "IsCake",
            //    table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Orders");

            //migrationBuilder.AddColumn<bool>(
            //    name: "IsCake",
            //    table: "Products",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);
        }
    }
}
