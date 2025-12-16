using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnLapTrinhWeb_QLyTiemBanh.Migrations
{
    /// <inheritdoc />
    public partial class InitUserCartSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserCartId",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserCarts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCarts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserCartId",
                table: "CartItems",
                column: "UserCartId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_UserCarts_UserCartId",
                table: "CartItems",
                column: "UserCartId",
                principalTable: "UserCarts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_UserCarts_UserCartId",
                table: "CartItems");

            migrationBuilder.DropTable(
                name: "UserCarts");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_UserCartId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "UserCartId",
                table: "CartItems");
        }
    }
}
