using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Electric_Power_Monitoring_System.Migrations
{
    /// <inheritdoc />
    public partial class AddLightingFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "ai_tips_cache",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "abnormal_consumption_tracking",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hub_serial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    plug_number = table.Column<int>(type: "integer", nullable: false),
                    stage = table.Column<int>(type: "integer", nullable: false),
                    stage_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_alert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    daily_consumption_wh = table.Column<decimal>(type: "numeric", nullable: true),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false),
                    resolved_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_abnormal_consumption_tracking", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "device_baseline",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hub_serial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    plug_number = table.Column<int>(type: "integer", nullable: false),
                    baseline_wh = table.Column<decimal>(type: "numeric", nullable: false),
                    calculated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_baseline", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lighting_estimates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<int>(type: "integer", nullable: false),
                    estimated_wh = table.Column<decimal>(type: "numeric", nullable: false),
                    actual_wh = table.Column<decimal>(type: "numeric", nullable: true),
                    is_corrected = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lighting_estimates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "meter_readings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reading_value_wh = table.Column<decimal>(type: "numeric", nullable: false),
                    balance_egp = table.Column<decimal>(type: "numeric", nullable: true),
                    reading_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meter_readings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_mandatory_state",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_mandatory_state", x => x.id);
                });

            migrationBuilder.UpdateData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 1,
                column: "type",
                value: "tier");

            migrationBuilder.UpdateData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 2,
                column: "type",
                value: "tier");

            migrationBuilder.UpdateData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 3,
                column: "type",
                value: "tier");

            migrationBuilder.UpdateData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 4,
                column: "type",
                value: "tier");

            migrationBuilder.UpdateData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 5,
                column: "type",
                value: "tier");

            migrationBuilder.UpdateData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 6,
                column: "type",
                value: "tier");

            migrationBuilder.UpdateData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 7,
                column: "type",
                value: "tier");

            migrationBuilder.InsertData(
                table: "ai_tips_cache",
                columns: new[] { "id", "is_active", "tip_text", "type" },
                values: new object[,]
                {
                    { 8, true, "افحص باب الثلاجة للتأكد من إغلاقه بإحكام.", "abnormal" },
                    { 9, true, "نظف المكثف الخلفي للثلاجة من الأتربة.", "abnormal" },
                    { 10, true, "تأكد من عدم وجود تسريب في غاز التبريد.", "abnormal" },
                    { 11, true, "افحص منظم الحرارة واضبطه على درجة حرارة مناسبة.", "abnormal" },
                    { 12, true, "تأكد من عدم وجود ثلج متراكم داخل الفريزر.", "abnormal" },
                    { 13, true, "افصل الثلاجة لمدة ساعة ثم أعد تشغيلها.", "abnormal" },
                    { 14, true, "تأكد من عدم وجود فجوات في عازل الباب.", "abnormal" },
                    { 15, true, "قم بقياس درجة الحرارة داخل الثلاجة للتأكد من أنها مناسبة.", "abnormal" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_abnormal_tracking_hub_plug",
                table: "abnormal_consumption_tracking",
                columns: new[] { "hub_serial", "plug_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_abnormal_tracking_stage",
                table: "abnormal_consumption_tracking",
                column: "stage");

            migrationBuilder.CreateIndex(
                name: "IX_device_baseline_hub_plug",
                table: "device_baseline",
                columns: new[] { "hub_serial", "plug_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lighting_estimates_user_year_month_day",
                table: "lighting_estimates",
                columns: new[] { "user_identifier", "year", "month", "day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_meter_readings_user_year_month",
                table: "meter_readings",
                columns: new[] { "user_identifier", "year", "month" });

            migrationBuilder.CreateIndex(
                name: "IX_user_mandatory_state_user",
                table: "user_mandatory_state",
                column: "user_identifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "abnormal_consumption_tracking");

            migrationBuilder.DropTable(
                name: "device_baseline");

            migrationBuilder.DropTable(
                name: "lighting_estimates");

            migrationBuilder.DropTable(
                name: "meter_readings");

            migrationBuilder.DropTable(
                name: "user_mandatory_state");

            migrationBuilder.DeleteData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ai_tips_cache",
                keyColumn: "id",
                keyValue: 15);

            migrationBuilder.DropColumn(
                name: "type",
                table: "ai_tips_cache");
        }
    }
}
