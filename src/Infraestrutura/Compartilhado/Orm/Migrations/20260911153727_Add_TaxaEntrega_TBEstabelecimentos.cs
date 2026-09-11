using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryApp.Infraestrutura.Compartilhado.Orm.Migrations
{
    /// <inheritdoc />
    public partial class Add_TaxaEntrega_TBEstabelecimentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TaxaEntrega",
                table: "TBEstabelecimentos",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxaEntrega",
                table: "TBEstabelecimentos");
        }
    }
}
