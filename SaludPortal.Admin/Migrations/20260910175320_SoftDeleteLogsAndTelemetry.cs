using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaludPortal.Admin.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteLogsAndTelemetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "telemetry_events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "telemetry_events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "logs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "logs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_is_deleted_timestamp",
                table: "telemetry_events",
                columns: new[] { "is_deleted", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_logs_is_deleted_timestamp",
                table: "logs",
                columns: new[] { "is_deleted", "timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_telemetry_is_deleted_timestamp",
                table: "telemetry_events");

            migrationBuilder.DropIndex(
                name: "IX_logs_is_deleted_timestamp",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "telemetry_events");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "telemetry_events");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "logs");
        }
    }
}
