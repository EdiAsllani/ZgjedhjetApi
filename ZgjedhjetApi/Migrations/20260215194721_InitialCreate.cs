using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZgjedhjetApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Zgjedhjet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kategoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Komuna = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Qendra_e_Votimit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VendVotimi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Partia111 = table.Column<int>(type: "int", nullable: false),
                    Partia112 = table.Column<int>(type: "int", nullable: false),
                    Partia113 = table.Column<int>(type: "int", nullable: false),
                    Partia114 = table.Column<int>(type: "int", nullable: false),
                    Partia115 = table.Column<int>(type: "int", nullable: false),
                    Partia116 = table.Column<int>(type: "int", nullable: false),
                    Partia117 = table.Column<int>(type: "int", nullable: false),
                    Partia118 = table.Column<int>(type: "int", nullable: false),
                    Partia119 = table.Column<int>(type: "int", nullable: false),
                    Partia120 = table.Column<int>(type: "int", nullable: false),
                    Partia121 = table.Column<int>(type: "int", nullable: false),
                    Partia122 = table.Column<int>(type: "int", nullable: false),
                    Partia123 = table.Column<int>(type: "int", nullable: false),
                    Partia124 = table.Column<int>(type: "int", nullable: false),
                    Partia125 = table.Column<int>(type: "int", nullable: false),
                    Partia126 = table.Column<int>(type: "int", nullable: false),
                    Partia127 = table.Column<int>(type: "int", nullable: false),
                    Partia128 = table.Column<int>(type: "int", nullable: false),
                    Partia129 = table.Column<int>(type: "int", nullable: false),
                    Partia130 = table.Column<int>(type: "int", nullable: false),
                    Partia131 = table.Column<int>(type: "int", nullable: false),
                    Partia132 = table.Column<int>(type: "int", nullable: false),
                    Partia133 = table.Column<int>(type: "int", nullable: false),
                    Partia134 = table.Column<int>(type: "int", nullable: false),
                    Partia135 = table.Column<int>(type: "int", nullable: false),
                    Partia136 = table.Column<int>(type: "int", nullable: false),
                    Partia137 = table.Column<int>(type: "int", nullable: false),
                    Partia138 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zgjedhjet", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Zgjedhjet");
        }
    }
}
