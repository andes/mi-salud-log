using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using SaludPortal.Admin.Entities;

namespace SaludPortal.Admin.Services.ExportFiles;

public class ExportCsvService : IExportFiles
{
    public MemoryStream GenerateLogs(IEnumerable<LogEntry> items)
    {
        var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        csv.WriteRecords(items.Select(l => new
        {
            l.Id,
            Timestamp = l.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss zzz"),
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
        }).ToList());

        writer.Flush();
        ms.Position = 0;
        return ms;
    }

    public MemoryStream GenerateTelemetry(IEnumerable<TelemetryEvent> items)
    {
        var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        csv.WriteRecords(items.Select(t => new
        {
            t.Id,
            Timestamp = t.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss zzz"),
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
        }).ToList());

        writer.Flush();
        ms.Position = 0;
        return ms;
    }
}