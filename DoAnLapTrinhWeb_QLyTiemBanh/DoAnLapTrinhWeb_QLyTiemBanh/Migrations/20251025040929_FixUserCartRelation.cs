using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnLapTrinhWeb_QLyTiemBanh.Migrations
{
    /// <inheritdoc />
    public partial class FixUserCartRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_UserCarts_UserCartId",
                table: "CartItems");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_UserCarts_UserCartId",
                table: "CartItems",
                column: "UserCartId",
                principalTable: "UserCarts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_UserCarts_UserCartId",
                table: "CartItems");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_UserCarts_UserCartId",
                table: "CartItems",
                column: "UserCartId",
                principalTable: "UserCarts",
                principalColumn: "Id");
        }
    }
}
