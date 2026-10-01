using System.Text.Json;
using SaludPortal.Admin.Entities;

namespace SaludPortal.Admin.Services.ExportFiles;

public class ExportJsonService : IExportFiles
{
    public MemoryStream GenerateLogs(IEnumerable<LogEntry> items)
    {
        var data = items.Select(l => new
        {
            l.Id,
            Timestamp = l.Timestamp.ToLocalTime().ToString("O"),
            l.Level,
            l.Category,
            l.Message,
            l.Exception,
            l.ClientIp,
            l.Latitude,
            l.Longitude,
            l.Browser,
            l.OsName,
            l.OsVersion,
            l.DeviceType,
            l.PatientId,
            l.SessionId,
        });
        var bytes = JsonSerializer.SerializeToUtf8Bytes(
            data, new JsonSerializerOptions { WriteIndented = true });
        return new MemoryStream(bytes);
    }

    public MemoryStream GenerateTelemetry(IEnumerable<TelemetryEvent> items)
    {
        var data = items.Select(t => new
        {
            t.Id,
            Timestamp = t.Timestamp.ToLocalTime().ToString("O"),
            t.EventType,
            t.Route,
            t.Target,
            t.PatientId,
            t.SessionId,
            t.Source,
            t.MetadataJson,
            t.ClientIp,
            t.Latitude,
            t.Longitude,
            t.Browser,
            t.OsName,
            t.OsVersion,
            t.DeviceType,
        });
        var bytes = JsonSerializer.SerializeToUtf8Bytes(
            data, new JsonSerializerOptions { WriteIndented = true });
        return new MemoryStream(bytes);
    }
}