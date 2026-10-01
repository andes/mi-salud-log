using SaludPortal.Admin.Services;
using SaludPortal.Admin.Services.ExportFiles;

namespace SaludPortal.Admin.Endpoints;

public static class ExportEndpoints
{
    private static readonly string[] AllowedFormats = ["csv", "json", "pdf"];

    public static void MapExportEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/export/logs", async (
            LogsService logsService,
            ExportCsvService csvExporter,
            ExportJsonService jsonExporter,
            ExportPdfService pdfExporter,
            string? format,
            string? level,
            string? dateFrom,
            string? dateTo,
            string? search,
            int? maxRows) =>
        {
            var fmt  = (format ?? "csv").ToLowerInvariant();
            if (!AllowedFormats.Contains(fmt))
                return Results.BadRequest("Formato inválido. Use: csv, json o pdf.");

            var rows   = Math.Clamp(maxRows ?? 1000, 1, 50_000);
            var from   = ParseDateTimeOffset(dateFrom);
            var to     = ParseDateTimeOffset(dateTo);
            var lvl    = string.IsNullOrWhiteSpace(level) ? null : level;
            var srch   = string.IsNullOrWhiteSpace(search) ? null : search;

            var items  = await logsService.GetAllForExportAsync(lvl, from, to, srch, rows);
            var stamp  = DateTime.Now.ToString("yyyy-MM-dd");

            return fmt switch
            {
                "json" => Results.File(
                    jsonExporter.GenerateLogs(items),
                    "application/json",
                    $"logs_{stamp}.json"),

                "pdf" => Results.File(
                    pdfExporter.GenerateLogs(items),
                    "application/pdf",
                    $"logs_{stamp}.pdf"),

                _ => Results.File(
                    csvExporter.GenerateLogs(items),
                    "text/csv; charset=utf-8",
                    $"logs_{stamp}.csv"),
            };
        })
        .RequireAuthorization()
        .DisableAntiforgery()
        .WithName("ExportLogs")
        .WithSummary("Export logs as CSV, JSON or PDF")
        .WithTags("Export");

        routes.MapGet("/api/export/telemetry", async (
            TelemetryService telemetryService,
            ExportCsvService csvExporter,
            ExportJsonService jsonExporter,
            ExportPdfService pdfExporter,
            string? format,
            string? eventType,
            string? patientId,
            string? dateFrom,
            string? dateTo,
            string? search,
            int? maxRows) =>
        {
            var fmt  = (format ?? "csv").ToLowerInvariant();
            if (!AllowedFormats.Contains(fmt))
                return Results.BadRequest("Formato inválido. Use: csv, json o pdf.");

            var rows = Math.Clamp(maxRows ?? 1000, 1, 50_000);
            var from = ParseDateTimeOffset(dateFrom);
            var to   = ParseDateTimeOffset(dateTo);
            var et   = string.IsNullOrWhiteSpace(eventType) ? null : eventType;
            var pid  = string.IsNullOrWhiteSpace(patientId) ? null : patientId;
            var srch = string.IsNullOrWhiteSpace(search) ? null : search;

            var items = await telemetryService.GetAllForExportAsync(et, pid, from, to, srch, rows);
            var stamp = DateTime.Now.ToString("yyyy-MM-dd");

            return fmt switch
            {
                "json" => Results.File(
                    jsonExporter.GenerateTelemetry(items),
                    "application/json",
                    $"telemetry_{stamp}.json"),

                "pdf" => Results.File(
                    pdfExporter.GenerateTelemetry(items),
                    "application/pdf",
                    $"telemetry_{stamp}.pdf"),

                _ => Results.File(
                    csvExporter.GenerateTelemetry(items),
                    "text/csv; charset=utf-8",
                    $"telemetry_{stamp}.csv"),
            };
        })
        .RequireAuthorization()
        .DisableAntiforgery()
        .WithName("ExportTelemetry")
        .WithSummary("Export telemetry events as CSV, JSON or PDF")
        .WithTags("Export");
    }

    private static DateTimeOffset? ParseDateTimeOffset(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return DateTimeOffset.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var result)
            ? result
            : null;
    }
}
