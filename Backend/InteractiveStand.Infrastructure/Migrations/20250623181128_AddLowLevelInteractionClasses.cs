using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InteractiveStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLowLevelInteractionClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PowerTransfers");

            migrationBuilder.CreateTable(
                name: "ConsumerBindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MacAddress = table.Column<string>(type: "text", nullable: false),
                    CapacityConsumerType = table.Column<int>(type: "integer", nullable: false),
                    RegionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumerBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumerBindings_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProducerBindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MacAddress = table.Column<string>(type: "text", nullable: false),
                    CapacityProducerType = table.Column<int>(type: "integer", nullable: false),
                    RegionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerBindings_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ConsumerBindings",
                columns: new[] { "Id", "CapacityConsumerType", "MacAddress", "RegionId" },
                values: new object[,]
                {
                    { 1, 0, "", 1 },
                    { 2, 1, "", 1 },
                    { 3, 0, "", 2 },
                    { 4, 1, "", 2 },
                    { 5, 0, "", 3 },
                    { 6, 1, "", 3 },
                    { 7, 0, "", 4 },
                    { 8, 1, "", 4 },
                    { 9, 0, "", 5 },
                    { 10, 1, "", 5 },
                    { 11, 0, "", 6 },
                    { 12, 1, "", 6 },
                    { 13, 0, "", 7 },
                    { 14, 1, "", 7 },
                    { 15, 0, "", 8 },
                    { 16, 1, "", 8 },
                    { 17, 0, "", 9 },
                    { 18, 1, "", 9 },
                    { 19, 0, "", 10 },
                    { 20, 1, "", 10 }
                });

            migrationBuilder.InsertData(
                table: "ProducerBindings",
                columns: new[] { "Id", "CapacityProducerType", "MacAddress", "RegionId" },
                values: new object[,]
                {
                    { 1, 0, "", 1 },
                    { 2, 2, "", 1 },
                    { 3, 1, "", 1 },
                    { 4, 3, "", 1 },
                    { 5, 4, "", 1 },
                    { 6, 0, "", 2 },
                    { 7, 2, "", 2 },
                    { 8, 1, "", 2 },
                    { 9, 3, "", 2 },
                    { 10, 4, "", 2 },
                    { 11, 0, "", 3 },
                    { 12, 2, "", 3 },
                    { 13, 1, "", 3 },
                    { 14, 3, "", 3 },
                    { 15, 4, "", 3 },
                    { 16, 0, "", 4 },
                    { 17, 2, "", 4 },
                    { 18, 1, "", 4 },
                    { 19, 3, "", 4 },
                    { 20, 4, "", 4 },
                    { 21, 0, "", 5 },
                    { 22, 2, "", 5 },
                    { 23, 1, "", 5 },
                    { 24, 3, "", 5 },
                    { 25, 4, "", 6 },
                    { 26, 0, "", 6 },
                    { 27, 2, "", 6 },
                    { 28, 1, "", 6 },
                    { 29, 3, "", 6 },
                    { 30, 4, "", 6 },
                    { 31, 0, "", 7 },
                    { 32, 2, "", 7 },
                    { 33, 1, "", 7 },
                    { 34, 3, "", 7 },
                    { 35, 4, "", 7 },
                    { 36, 0, "", 8 },
                    { 37, 2, "", 8 },
                    { 38, 1, "", 8 },
                    { 39, 3, "", 8 },
                    { 40, 4, "", 8 },
                    { 41, 0, "", 9 },
                    { 42, 2, "", 9 },
                    { 43, 1, "", 9 },
                    { 44, 3, "", 9 },
                    { 45, 4, "", 9 },
                    { 46, 0, "", 10 },
                    { 47, 2, "", 10 },
                    { 48, 1, "", 10 },
                    { 49, 3, "", 10 },
                    { 50, 4, "", 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerBindings_RegionId",
                table: "ConsumerBindings",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerBindings_RegionId",
                table: "ProducerBindings",
                column: "RegionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumerBindings");

            migrationBuilder.DropTable(
                name: "ProducerBindings");

            migrationBuilder.CreateTable(
                name: "PowerTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false),
                    SentCapacity = table.Column<double>(type: "double precision", nullable: false),
                    WhoReceivedId = table.Column<int>(type: "integer", nullable: false),
                    WhoSentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerTransfers_Regions_WhoReceivedId",
                        column: x => x.WhoReceivedId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PowerTransfers_Regions_WhoSentId",
                        column: x => x.WhoSentId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PowerTransfers_WhoReceivedId",
                table: "PowerTransfers",
                column: "WhoReceivedId");

            migrationBuilder.CreateIndex(
                name: "IX_PowerTransfers_WhoSentId",
                table: "PowerTransfers",
                column: "WhoSentId");
        }
    }
}
