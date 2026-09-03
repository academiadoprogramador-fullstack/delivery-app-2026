using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryApp.Infraestrutura.Compartilhado.Orm.Migrations
{
    /// <inheritdoc />
    public partial class Add_Estabelecimentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBEstabelecimentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeComercial = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NomeComercialNormalizado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Endereco = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    HorarioAbertura = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HorarioFechamento = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    AreaAtendimento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBEstabelecimentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBEstabelecimentos_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { new Guid("019958f4-a048-70a3-b1a6-0f01d629a127"), "019958f7-9492-73bc-8e4b-934c53594ed7", "Estabelecimento", "ESTABELECIMENTO" });

            migrationBuilder.CreateIndex(
                name: "IX_TBEstabelecimentos_NomeComercialNormalizado",
                table: "TBEstabelecimentos",
                column: "NomeComercialNormalizado",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBEstabelecimentos");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("019958f4-a048-70a3-b1a6-0f01d629a127"));
        }
    }
}
