using SaludPortal.Admin.Entities;

namespace SaludPortal.Admin.Services.ExportFiles;

public interface IExportFiles
{
    MemoryStream GenerateLogs(IEnumerable<LogEntry> items);
    MemoryStream GenerateTelemetry(IEnumerable<TelemetryEvent> items);
}