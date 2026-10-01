using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaludPortal.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddClientContextFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "browser",
                table: "telemetry_events",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "client_ip",
                table: "telemetry_events",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "device_type",
                table: "telemetry_events",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "telemetry_events",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "telemetry_events",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "os_name",
                table: "telemetry_events",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "os_version",
                table: "telemetry_events",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_agent",
                table: "telemetry_events",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "browser",
                table: "logs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "client_ip",
                table: "logs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "device_type",
                table: "logs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "logs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "logs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "os_name",
                table: "logs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "os_version",
                table: "logs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "patient_id",
                table: "logs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_id",
                table: "logs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_agent",
                table: "logs",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_events_client_ip",
                table: "telemetry_events",
                column: "client_ip");

            migrationBuilder.CreateIndex(
                name: "IX_logs_client_ip",
                table: "logs",
                column: "client_ip");

            migrationBuilder.CreateIndex(
                name: "IX_logs_patient_id",
                table: "logs",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_logs_timestamp",
                table: "logs",
                column: "timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_telemetry_events_client_ip",
                table: "telemetry_events");

            migrationBuilder.DropIndex(
                name: "IX_logs_client_ip",
                table: "logs");

            migrationBuilder.DropIndex(
                name: "IX_logs_patient_id",
                table: "logs");

            migrationBuilder.DropIndex(
                name: "IX_logs_timestamp",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "browser",
                table: "telemetry_events");

            migrationBuilder.DropColumn(
                name: "client_ip",
                table: "telemetry_events");

            migrationBuilder.DropColumn(
                name: "device_type",
                table: "telemetry_events");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "telemetry_events");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "telemetry_events");

            migrationBuilder.DropColumn(
                name: "os_name",
                table: "telemetry_events");

            migrationBuilder.DropColumn(
                name: "os_version",
                table: "telemetry_events");

            migrationBuilder.DropColumn(
                name: "user_agent",
                table: "telemetry_events");

            migrationBuilder.DropColumn(
                name: "browser",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "client_ip",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "device_type",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "os_name",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "os_version",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "patient_id",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "session_id",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "user_agent",
                table: "logs");
        }
    }
}
