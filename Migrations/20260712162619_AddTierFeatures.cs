using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Electric_Power_Monitoring_System.Migrations
{
    /// <inheritdoc />
    public partial class AddTierFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_tips_cache",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tip_text = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_tips_cache", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tier_notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    remaining_kwh = table.Column<decimal>(type: "numeric", nullable: false),
                    next_tier_price = table.Column<decimal>(type: "numeric", nullable: false),
                    tips = table.Column<string>(type: "text", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tier_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tier_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tier_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    min_kwh = table.Column<decimal>(type: "numeric", nullable: false),
                    max_kwh = table.Column<decimal>(type: "numeric", nullable: false),
                    price_per_kwh = table.Column<decimal>(type: "numeric", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tier_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_consumption_tracking",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    cumulative_consumption_wh = table.Column<decimal>(type: "numeric", nullable: false),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_consumption_tracking", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "ai_tips_cache",
                columns: new[] { "id", "is_active", "tip_text" },
                values: new object[,]
                {
                    { 1, true, "أطفئ الأجهزة غير المستخدمة من الفيشة لتوفير الكهرباء." },
                    { 2, true, "استخدم لمبات LED بدلاً من اللمبات العادية." },
                    { 3, true, "اضبط درجة حرارة الثلاجة على المستوى المتوسط." },
                    { 4, true, "شغل الغسالة والغسالة الأطباق في أوقات الذروة المنخفضة (بعد 10 مساءً)." },
                    { 5, true, "قلل من استخدام المكواة الكهربائية أو استخدمها لفترات قصيرة." },
                    { 6, true, "استخدم سخان المياه الشمسي بدلاً من الكهربائي." },
                    { 7, true, "افصل شاحن الهاتف بعد شحن البطارية." }
                });

            migrationBuilder.InsertData(
                table: "tier_settings",
                columns: new[] { "id", "is_active", "max_kwh", "min_kwh", "price_per_kwh", "tier_name" },
                values: new object[,]
                {
                    { 1, true, 50m, 0m, 0.68m, "شريحة 1" },
                    { 2, true, 100m, 51m, 0.78m, "شريحة 2" },
                    { 3, true, 200m, 101m, 0.95m, "شريحة 3" },
                    { 4, true, 350m, 201m, 1.20m, "شريحة 4" },
                    { 5, true, 650m, 351m, 1.45m, "شريحة 5" },
                    { 6, true, 1000m, 651m, 1.60m, "شريحة 6" },
                    { 7, true, 79228162514264337593543950335m, 1001m, 1.85m, "شريحة 7" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tier_notifications_sent_at",
                table: "tier_notifications",
                column: "sent_at");

            migrationBuilder.CreateIndex(
                name: "IX_tier_notifications_user",
                table: "tier_notifications",
                column: "user_identifier");

            migrationBuilder.CreateIndex(
                name: "IX_user_consumption_tracking_user_year_month",
                table: "user_consumption_tracking",
                columns: new[] { "user_identifier", "year", "month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_tips_cache");

            migrationBuilder.DropTable(
                name: "tier_notifications");

            migrationBuilder.DropTable(
                name: "tier_settings");

            migrationBuilder.DropTable(
                name: "user_consumption_tracking");
        }
    }
}
