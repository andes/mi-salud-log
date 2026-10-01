using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaludPortal.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_telemetry_device_type",
                table: "telemetry_events",
                column: "device_type");

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_event_type_timestamp",
                table: "telemetry_events",
                columns: new[] { "event_type", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_timestamp_lat_lon",
                table: "telemetry_events",
                columns: new[] { "timestamp", "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_logs_level_timestamp",
                table: "logs",
                columns: new[] { "level", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_logs_timestamp_lat_lon",
                table: "logs",
                columns: new[] { "timestamp", "latitude", "longitude" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_telemetry_device_type",
                table: "telemetry_events");

            migrationBuilder.DropIndex(
                name: "IX_telemetry_event_type_timestamp",
                table: "telemetry_events");

            migrationBuilder.DropIndex(
                name: "IX_telemetry_timestamp_lat_lon",
                table: "telemetry_events");

            migrationBuilder.DropIndex(
                name: "IX_logs_level_timestamp",
                table: "logs");

            migrationBuilder.DropIndex(
                name: "IX_logs_timestamp_lat_lon",
                table: "logs");
        }
    }
}
