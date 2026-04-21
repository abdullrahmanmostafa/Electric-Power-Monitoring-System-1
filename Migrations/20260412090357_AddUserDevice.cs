using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Electric_Power_Monitoring_System.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hubs",
                columns: table => new
                {
                    serial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_seen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hubs", x => x.serial);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    hub_serial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    plug_number = table.Column<int>(type: "integer", nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fcm_response = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_hubs_hub_serial",
                        column: x => x.hub_serial,
                        principalTable: "hubs",
                        principalColumn: "serial",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "plugs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hub_serial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    plug_number = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plugs", x => x.id);
                    table.ForeignKey(
                        name: "FK_plugs_hubs_hub_serial",
                        column: x => x.hub_serial,
                        principalTable: "hubs",
                        principalColumn: "serial",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "readings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hub_serial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    plug_number = table.Column<int>(type: "integer", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cumulative_energy_wh = table.Column<decimal>(type: "numeric", nullable: false),
                    state = table.Column<bool>(type: "boolean", nullable: false),
                    PlugId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_readings", x => x.id);
                    table.ForeignKey(
                        name: "FK_readings_hubs_hub_serial",
                        column: x => x.hub_serial,
                        principalTable: "hubs",
                        principalColumn: "serial",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_readings_plugs_PlugId",
                        column: x => x.PlugId,
                        principalTable: "plugs",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_hubs_user_id",
                table: "hubs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_hub_serial",
                table: "notifications",
                column: "hub_serial");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_sent_at",
                table: "notifications",
                column: "sent_at");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_plugs_hub_serial_plug_number",
                table: "plugs",
                columns: new[] { "hub_serial", "plug_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_readings_hub_plug_timestamp",
                table: "readings",
                columns: new[] { "hub_serial", "plug_number", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_readings_PlugId",
                table: "readings",
                column: "PlugId");

            migrationBuilder.CreateIndex(
                name: "IX_readings_timestamp",
                table: "readings",
                column: "timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "readings");

            migrationBuilder.DropTable(
                name: "plugs");

            migrationBuilder.DropTable(
                name: "hubs");
        }
    }
}
