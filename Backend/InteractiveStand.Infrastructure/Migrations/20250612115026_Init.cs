using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InteractiveStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConnectedRegions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RegionSourceId = table.Column<int>(type: "integer", nullable: false),
                    RegionDestinationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectedRegions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Consumers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstPercentage = table.Column<double>(type: "double precision", nullable: false),
                    SecondPercentage = table.Column<double>(type: "double precision", nullable: false),
                    ThirdPercentage = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PowerSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AESPercentage = table.Column<double>(type: "double precision", nullable: false),
                    GESPercentage = table.Column<double>(type: "double precision", nullable: false),
                    TESPercentage = table.Column<double>(type: "double precision", nullable: false),
                    VESPercentage = table.Column<double>(type: "double precision", nullable: false),
                    SESPercentage = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProducedCapacity = table.Column<double>(type: "double precision", nullable: false),
                    ConsumedCapacity = table.Column<double>(type: "double precision", nullable: false),
                    PowerSourceId = table.Column<int>(type: "integer", nullable: false),
                    ConsumerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regions_Consumers_ConsumerId",
                        column: x => x.ConsumerId,
                        principalTable: "Consumers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Regions_PowerSources_PowerSourceId",
                        column: x => x.PowerSourceId,
                        principalTable: "PowerSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ConnectedRegions",
                columns: new[] { "Id", "RegionDestinationId", "RegionSourceId" },
                values: new object[,]
                {
                    { 1, 2, 1 },
                    { 2, 5, 1 },
                    { 3, 8, 1 },
                    { 4, 1, 2 },
                    { 5, 3, 2 },
                    { 6, 4, 2 },
                    { 7, 5, 2 },
                    { 8, 2, 3 },
                    { 9, 4, 3 },
                    { 10, 2, 4 },
                    { 11, 3, 4 },
                    { 12, 5, 4 },
                    { 13, 10, 4 },
                    { 14, 1, 5 },
                    { 15, 2, 5 },
                    { 16, 4, 5 },
                    { 17, 6, 5 },
                    { 18, 5, 6 },
                    { 19, 7, 6 },
                    { 20, 9, 6 },
                    { 21, 6, 7 },
                    { 22, 1, 8 },
                    { 23, 6, 9 },
                    { 24, 4, 10 }
                });

            migrationBuilder.InsertData(
                table: "Consumers",
                columns: new[] { "Id", "FirstPercentage", "SecondPercentage", "ThirdPercentage" },
                values: new object[,]
                {
                    { 1, 30.0, 20.0, 50.0 },
                    { 2, 40.0, 20.0, 40.0 },
                    { 3, 5.0, 25.0, 70.0 },
                    { 4, 20.0, 10.0, 70.0 },
                    { 5, 20.0, 20.0, 60.0 },
                    { 6, 60.0, 20.0, 20.0 },
                    { 7, 20.0, 50.0, 30.0 },
                    { 8, 30.0, 20.0, 50.0 },
                    { 9, 70.0, 10.0, 20.0 },
                    { 10, 30.0, 20.0, 50.0 }
                });

            migrationBuilder.InsertData(
                table: "PowerSources",
                columns: new[] { "Id", "AESPercentage", "GESPercentage", "SESPercentage", "TESPercentage", "VESPercentage" },
                values: new object[,]
                {
                    { 1, 25.0, 4.0, 0.0, 71.0, 0.0 },
                    { 2, 15.0, 25.0, 0.69999999999999996, 59.0, 0.29999999999999999 },
                    { 3, 3.0, 4.0, 1.0, 92.0, 0.0 },
                    { 4, 24.0, 12.0, 0.0, 63.0, 1.0 },
                    { 5, 23.0, 15.0, 4.0, 50.0, 8.0 },
                    { 6, 0.0, 48.0, 1.0, 51.0, 0.0 },
                    { 7, 0.0, 41.0, 0.0, 59.0, 0.0 },
                    { 8, 2.0, 48.0, 0.0, 50.0, 0.0 },
                    { 9, 0.0, 0.0, 50.0, 50.0, 0.0 },
                    { 10, 0.0, 0.0, 0.0, 0.0, 100.0 }
                });

            migrationBuilder.InsertData(
                table: "Regions",
                columns: new[] { "Id", "ConsumedCapacity", "ConsumerId", "Name", "PowerSourceId", "ProducedCapacity" },
                values: new object[,]
                {
                    { 1, 100.0, 1, "ОЭС 1", 1, 23.0 },
                    { 2, 270.0, 2, "ОЭС 2", 2, 62.0 },
                    { 3, 140.0, 3, "ОЭС 3", 3, 32.0 },
                    { 4, 120.0, 4, "ОЭС 4", 4, 27.0 },
                    { 5, 260.0, 5, "ОЭС 5", 5, 60.0 },
                    { 6, 240.0, 6, "ОЭС 6", 6, 55.0 },
                    { 7, 11.0, 7, "ОЭС 7", 7, 50.0 },
                    { 8, 3.0, 8, "АЭК-ТИТЭС", 8, 15.0 },
                    { 9, 3.0, 9, "АЭК-ПРОМ", 9, 15.0 },
                    { 10, 0.5, 10, "АЭК-ВИЭ", 10, 10.0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Regions_ConsumerId",
                table: "Regions",
                column: "ConsumerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_PowerSourceId",
                table: "Regions",
                column: "PowerSourceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectedRegions");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "Consumers");

            migrationBuilder.DropTable(
                name: "PowerSources");
        }
    }
}
