using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InteractiveStand.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsToPowerSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VESPercentage",
                table: "PowerSources",
                newName: "WPP_Percentage");

            migrationBuilder.RenameColumn(
                name: "TESPercentage",
                table: "PowerSources",
                newName: "WPP_LoadFactor");

            migrationBuilder.RenameColumn(
                name: "SESPercentage",
                table: "PowerSources",
                newName: "WPP_Capacity");

            migrationBuilder.RenameColumn(
                name: "GESPercentage",
                table: "PowerSources",
                newName: "SPP_Percentage");

            migrationBuilder.RenameColumn(
                name: "AESPercentage",
                table: "PowerSources",
                newName: "SPP_LoadFactor");

            migrationBuilder.AddColumn<double>(
                name: "CGPP_Capacity",
                table: "PowerSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CGPP_LoadFactor",
                table: "PowerSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CGPP_Percentage",
                table: "PowerSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "HPP_Capacity",
                table: "PowerSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "HPP_LoadFactor",
                table: "PowerSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "HPP_Percentage",
                table: "PowerSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NPP_Capacity",
                table: "PowerSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NPP_LoadFactor",
                table: "PowerSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NPP_Percentage",
                table: "PowerSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SPP_Capacity",
                table: "PowerSources",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CGPP_Capacity", "CGPP_LoadFactor", "CGPP_Percentage", "HPP_Capacity", "HPP_LoadFactor", "HPP_Percentage", "NPP_Capacity", "NPP_LoadFactor", "NPP_Percentage", "SPP_Capacity", "SPP_LoadFactor", "SPP_Percentage", "WPP_LoadFactor" },
                values: new object[] { 16.329999999999998, 100.0, 71.0, 0.92000000000000004, 100.0, 4.0, 5.75, 100.0, 25.0, 0.0, 100.0, 0.0, 100.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CGPP_Capacity", "CGPP_LoadFactor", "CGPP_Percentage", "HPP_Capacity", "HPP_LoadFactor", "HPP_Percentage", "NPP_Capacity", "NPP_LoadFactor", "NPP_Percentage", "SPP_Capacity", "SPP_LoadFactor", "SPP_Percentage", "WPP_Capacity", "WPP_LoadFactor" },
                values: new object[] { 36.579999999999998, 100.0, 59.0, 15.5, 100.0, 25.0, 9.3000000000000007, 100.0, 15.0, 0.434, 100.0, 0.69999999999999996, 0.18599999999999997, 100.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CGPP_Capacity", "CGPP_LoadFactor", "CGPP_Percentage", "HPP_Capacity", "HPP_LoadFactor", "HPP_Percentage", "NPP_Capacity", "NPP_LoadFactor", "NPP_Percentage", "SPP_Capacity", "SPP_LoadFactor", "SPP_Percentage", "WPP_Capacity", "WPP_LoadFactor" },
                values: new object[] { 29.440000000000001, 100.0, 92.0, 1.28, 100.0, 4.0, 0.95999999999999996, 100.0, 3.0, 0.32000000000000001, 100.0, 1.0, 0.0, 100.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CGPP_Capacity", "CGPP_LoadFactor", "CGPP_Percentage", "HPP_Capacity", "HPP_LoadFactor", "HPP_Percentage", "NPP_Capacity", "NPP_LoadFactor", "NPP_Percentage", "SPP_Capacity", "SPP_LoadFactor", "SPP_Percentage", "WPP_Capacity", "WPP_LoadFactor" },
                values: new object[] { 17.010000000000002, 100.0, 63.0, 3.2400000000000002, 100.0, 12.0, 6.4800000000000004, 100.0, 24.0, 0.0, 100.0, 0.0, 0.27000000000000002, 100.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CGPP_Capacity", "CGPP_LoadFactor", "CGPP_Percentage", "HPP_Capacity", "HPP_LoadFactor", "HPP_Percentage", "NPP_Capacity", "NPP_LoadFactor", "NPP_Percentage", "SPP_Capacity", "SPP_LoadFactor", "SPP_Percentage", "WPP_Capacity", "WPP_LoadFactor" },
                values: new object[] { 30.0, 100.0, 50.0, 9.0, 100.0, 15.0, 13.800000000000001, 100.0, 23.0, 2.3999999999999999, 100.0, 4.0, 4.7999999999999998, 100.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CGPP_Capacity", "CGPP_LoadFactor", "CGPP_Percentage", "HPP_Capacity", "HPP_LoadFactor", "HPP_Percentage", "NPP_Capacity", "NPP_LoadFactor", "NPP_Percentage", "SPP_Capacity", "SPP_LoadFactor", "SPP_Percentage", "WPP_Capacity", "WPP_LoadFactor" },
                values: new object[] { 28.050000000000001, 100.0, 51.0, 26.399999999999999, 100.0, 48.0, 0.0, 100.0, 0.0, 0.55000000000000004, 100.0, 1.0, 0.0, 100.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CGPP_Capacity", "CGPP_LoadFactor", "CGPP_Percentage", "HPP_Capacity", "HPP_LoadFactor", "HPP_Percentage", "NPP_Capacity", "NPP_LoadFactor", "NPP_Percentage", "SPP_Capacity", "SPP_LoadFactor", "SPP_Percentage", "WPP_LoadFactor" },
                values: new object[] { 29.5, 100.0, 59.0, 20.5, 100.0, 41.0, 0.0, 100.0, 0.0, 0.0, 100.0, 0.0, 100.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CGPP_Capacity", "CGPP_LoadFactor", "CGPP_Percentage", "HPP_Capacity", "HPP_LoadFactor", "HPP_Percentage", "NPP_Capacity", "NPP_LoadFactor", "NPP_Percentage", "SPP_Capacity", "SPP_LoadFactor", "SPP_Percentage", "WPP_LoadFactor" },
                values: new object[] { 7.5, 100.0, 50.0, 7.2000000000000002, 100.0, 48.0, 0.29999999999999999, 100.0, 2.0, 0.0, 100.0, 0.0, 100.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CGPP_Capacity", "CGPP_LoadFactor", "CGPP_Percentage", "HPP_Capacity", "HPP_LoadFactor", "HPP_Percentage", "NPP_Capacity", "NPP_LoadFactor", "NPP_Percentage", "SPP_Capacity", "SPP_LoadFactor", "SPP_Percentage", "WPP_Capacity", "WPP_LoadFactor" },
                values: new object[] { 7.5, 100.0, 50.0, 0.0, 100.0, 0.0, 0.0, 100.0, 0.0, 7.5, 100.0, 50.0, 0.0, 100.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CGPP_Capacity", "CGPP_LoadFactor", "CGPP_Percentage", "HPP_Capacity", "HPP_LoadFactor", "HPP_Percentage", "NPP_Capacity", "NPP_LoadFactor", "NPP_Percentage", "SPP_Capacity", "SPP_LoadFactor", "WPP_Capacity", "WPP_LoadFactor" },
                values: new object[] { 0.0, 100.0, 0.0, 0.0, 100.0, 0.0, 0.0, 100.0, 0.0, 0.0, 100.0, 10.0, 100.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CGPP_Capacity",
                table: "PowerSources");

            migrationBuilder.DropColumn(
                name: "CGPP_LoadFactor",
                table: "PowerSources");

            migrationBuilder.DropColumn(
                name: "CGPP_Percentage",
                table: "PowerSources");

            migrationBuilder.DropColumn(
                name: "HPP_Capacity",
                table: "PowerSources");

            migrationBuilder.DropColumn(
                name: "HPP_LoadFactor",
                table: "PowerSources");

            migrationBuilder.DropColumn(
                name: "HPP_Percentage",
                table: "PowerSources");

            migrationBuilder.DropColumn(
                name: "NPP_Capacity",
                table: "PowerSources");

            migrationBuilder.DropColumn(
                name: "NPP_LoadFactor",
                table: "PowerSources");

            migrationBuilder.DropColumn(
                name: "NPP_Percentage",
                table: "PowerSources");

            migrationBuilder.DropColumn(
                name: "SPP_Capacity",
                table: "PowerSources");

            migrationBuilder.RenameColumn(
                name: "WPP_Percentage",
                table: "PowerSources",
                newName: "VESPercentage");

            migrationBuilder.RenameColumn(
                name: "WPP_LoadFactor",
                table: "PowerSources",
                newName: "TESPercentage");

            migrationBuilder.RenameColumn(
                name: "WPP_Capacity",
                table: "PowerSources",
                newName: "SESPercentage");

            migrationBuilder.RenameColumn(
                name: "SPP_Percentage",
                table: "PowerSources",
                newName: "GESPercentage");

            migrationBuilder.RenameColumn(
                name: "SPP_LoadFactor",
                table: "PowerSources",
                newName: "AESPercentage");

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AESPercentage", "GESPercentage", "TESPercentage" },
                values: new object[] { 25.0, 4.0, 71.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AESPercentage", "GESPercentage", "SESPercentage", "TESPercentage" },
                values: new object[] { 15.0, 25.0, 0.69999999999999996, 59.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AESPercentage", "GESPercentage", "SESPercentage", "TESPercentage" },
                values: new object[] { 3.0, 4.0, 1.0, 92.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AESPercentage", "GESPercentage", "SESPercentage", "TESPercentage" },
                values: new object[] { 24.0, 12.0, 0.0, 63.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AESPercentage", "GESPercentage", "SESPercentage", "TESPercentage" },
                values: new object[] { 23.0, 15.0, 4.0, 50.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AESPercentage", "GESPercentage", "SESPercentage", "TESPercentage" },
                values: new object[] { 0.0, 48.0, 1.0, 51.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AESPercentage", "GESPercentage", "TESPercentage" },
                values: new object[] { 0.0, 41.0, 59.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AESPercentage", "GESPercentage", "TESPercentage" },
                values: new object[] { 2.0, 48.0, 50.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AESPercentage", "GESPercentage", "SESPercentage", "TESPercentage" },
                values: new object[] { 0.0, 0.0, 50.0, 50.0 });

            migrationBuilder.UpdateData(
                table: "PowerSources",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "AESPercentage", "SESPercentage", "TESPercentage" },
                values: new object[] { 0.0, 0.0, 0.0 });
        }
    }
}
