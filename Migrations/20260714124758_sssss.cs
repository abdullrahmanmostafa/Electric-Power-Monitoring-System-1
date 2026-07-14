using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Electric_Power_Monitoring_System.Migrations
{
    /// <inheritdoc />
    public partial class sssss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "cumulative_consumption_wh",
                table: "user_consumption_tracking",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "price_per_kwh",
                table: "tier_settings",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "min_kwh",
                table: "tier_settings",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "max_kwh",
                table: "tier_settings",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "remaining_kwh",
                table: "tier_notifications",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "next_tier_price",
                table: "tier_notifications",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "cumulative_energy_wh",
                table: "readings",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "reading_value_wh",
                table: "meter_readings",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "balance_egp",
                table: "meter_readings",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "estimated_wh",
                table: "lighting_estimates",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "actual_wh",
                table: "lighting_estimates",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "baseline_wh",
                table: "device_baseline",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "daily_consumption_wh",
                table: "abnormal_consumption_tracking",
                type: "numeric(20,10)",
                precision: 20,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "cumulative_consumption_wh",
                table: "user_consumption_tracking",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10);

            migrationBuilder.AlterColumn<decimal>(
                name: "price_per_kwh",
                table: "tier_settings",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10);

            migrationBuilder.AlterColumn<decimal>(
                name: "min_kwh",
                table: "tier_settings",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10);

            migrationBuilder.AlterColumn<decimal>(
                name: "max_kwh",
                table: "tier_settings",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10);

            migrationBuilder.AlterColumn<decimal>(
                name: "remaining_kwh",
                table: "tier_notifications",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10);

            migrationBuilder.AlterColumn<decimal>(
                name: "next_tier_price",
                table: "tier_notifications",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10);

            migrationBuilder.AlterColumn<decimal>(
                name: "cumulative_energy_wh",
                table: "readings",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10);

            migrationBuilder.AlterColumn<decimal>(
                name: "reading_value_wh",
                table: "meter_readings",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10);

            migrationBuilder.AlterColumn<decimal>(
                name: "balance_egp",
                table: "meter_readings",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "estimated_wh",
                table: "lighting_estimates",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10);

            migrationBuilder.AlterColumn<decimal>(
                name: "actual_wh",
                table: "lighting_estimates",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "baseline_wh",
                table: "device_baseline",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10);

            migrationBuilder.AlterColumn<decimal>(
                name: "daily_consumption_wh",
                table: "abnormal_consumption_tracking",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,10)",
                oldPrecision: 20,
                oldScale: 10,
                oldNullable: true);
        }
    }
}
