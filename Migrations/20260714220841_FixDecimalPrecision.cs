using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Electric_Power_Monitoring_System.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "max_kwh",
                table: "tier_settings",
                type: "numeric(38,10)",
                precision: 38,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "max_kwh",
                table: "tier_settings",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(38,10)",
                oldPrecision: 38,
                oldScale: 10);
        }
    }
}
