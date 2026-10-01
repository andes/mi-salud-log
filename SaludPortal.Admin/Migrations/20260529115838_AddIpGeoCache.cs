using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaludPortal.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddIpGeoCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ip_geo_cache",
                columns: table => new
                {
                    ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    fetched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ip_geo_cache", x => x.ip);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ip_geo_cache_fetched_at",
                table: "ip_geo_cache",
                column: "fetched_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ip_geo_cache");
        }
    }
}
