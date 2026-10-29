using System.Globalization;
using System.Text;
using CsvHelper;
using SaludPortal.Admin.Entities;

namespace SaludPortal.Admin.Services.ExportFiles;

public class ExportCsvService : IExportFiles
{
    private static readonly CsvWriterOptions WriterOptions = new()
    {
        FormatProvider = CultureInfo.InvariantCulture,
    };

    private static readonly string[] LogColumns =
    [
        "Id", "Timestamp", "Level", "Category", "Message", "Exception", "ClientIp",
        "Latitude", "Longitude", "Browser", "OsName", "OsVersion", "DeviceType",
        "PatientId", "SessionId",
    ];

    private static readonly string[] TelemetryColumns =
    [
        "Id", "Timestamp", "EventType", "Route", "Target", "PatientId", "SessionId",
        "Source", "MetadataJson", "ClientIp", "Latitude", "Longitude", "Browser",
        "OsName", "OsVersion", "DeviceType",
    ];

    public MemoryStream GenerateLogs(IEnumerable<LogEntry> items) =>
        Write(LogColumns, items.Select(l => new string?[]
        {
            l.Id.ToString(CultureInfo.InvariantCulture),
            FormatTimestamp(l.Timestamp),
            l.Level,
            l.Category,
            l.Message,
            l.Exception,
            l.ClientIp,
            FormatCoordinate(l.Latitude),
            FormatCoordinate(l.Longitude),
            l.Browser,
            l.OsName,
            l.OsVersion,
            l.DeviceType,
            l.PatientId,
            l.SessionId,
        }));

    public MemoryStream GenerateTelemetry(IEnumerable<TelemetryEvent> items) =>
        Write(TelemetryColumns, items.Select(t => new string?[]
        {
            t.Id.ToString(CultureInfo.InvariantCulture),
            FormatTimestamp(t.Timestamp),
            t.EventType,
            t.Route,
            t.Target,
            t.PatientId,
            t.SessionId,
            t.Source,
            t.MetadataJson,
            t.ClientIp,
            FormatCoordinate(t.Latitude),
            FormatCoordinate(t.Longitude),
            t.Browser,
            t.OsName,
            t.OsVersion,
            t.DeviceType,
        }));

    private static MemoryStream Write(string[] columns, IEnumerable<string?[]> rows)
    {
        using var ms = new MemoryStream();
        using (var writer = new StreamWriter(ms, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true))
        using (var csv = CsvWriter.Create(writer, WriterOptions, leaveOpen: true))
        {
            csv.WriteHeader(columns);
            foreach (var row in rows)
                csv.WriteRecord(row!);
        }

        return new MemoryStream(ms.ToArray());
    }

    private static string FormatTimestamp(DateTimeOffset timestamp) =>
        timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture);

    private static string? FormatCoordinate(double? value) =>
        value?.ToString(CultureInfo.InvariantCulture);
}
