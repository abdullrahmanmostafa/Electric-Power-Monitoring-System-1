using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Electric_Power_Monitoring_System.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileFieldsToUserDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "user_devices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "first_name",
                table: "user_devices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_name",
                table: "user_devices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "user_devices",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "user_devices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email",
                table: "user_devices");

            migrationBuilder.DropColumn(
                name: "first_name",
                table: "user_devices");

            migrationBuilder.DropColumn(
                name: "last_name",
                table: "user_devices");

            migrationBuilder.DropColumn(
                name: "password",
                table: "user_devices");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "user_devices");
        }
    }
}
