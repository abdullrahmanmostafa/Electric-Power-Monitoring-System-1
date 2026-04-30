using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Electric_Power_Monitoring_System.Migrations
{
    /// <inheritdoc />
    public partial class AddUserHubAndRemoveHubUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hubs_user_id",
                table: "hubs");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "hubs");

            migrationBuilder.CreateTable(
                name: "user_hubs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    hub_serial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_hubs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_hubs_user_hub",
                table: "user_hubs",
                columns: new[] { "user_identifier", "hub_serial" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_hubs");

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "hubs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_hubs_user_id",
                table: "hubs",
                column: "user_id");
        }
    }
}
