using Microsoft.EntityFrameworkCore;
using SaludPortal.Admin.Data;
using SaludPortal.Admin.Entities;
using SaludPortal.Admin.Models;
using SaludPortal.Admin.Utils;

namespace SaludPortal.Admin.Services;

public class DashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStats> GetStatsHoyAsync()
    {
        var desde = ArgentinaTimeZone.StartOfTodayUtc();

        var eventosPorTipo = await _context.TelemetryEvents
            .Where(t => t.Timestamp >= desde)
            .GroupBy(t => t.EventType)
            .Select(g => new { EventType = g.Key, Count = g.Count() })
            .ToListAsync();

        var eventosHoy = eventosPorTipo.Sum(x => x.Count);
        var eventosDict = eventosPorTipo.ToDictionary(x => x.EventType, x => x.Count);

        var pacientesHoy = await _context.TelemetryEvents
            .Where(t => t.Timestamp >= desde && t.PatientId != null)
            .Select(t => t.PatientId!)
            .Distinct()
            .CountAsync();

        var erroresHoy = await _context.Logs
            .Where(l => l.Timestamp >= desde && (l.Level == "Error" || l.Level == "Critical"))
            .CountAsync();

        return new DashboardStats(eventosHoy, eventosDict, pacientesHoy, erroresHoy);
    }

    public async Task<List<TopPagina>> GetTopPaginasAsync(int dias = 7, int top = 8)
    {
        var desde = DateTimeOffset.UtcNow.AddDays(-dias);

        var rows = await _context.TelemetryEvents
            .Where(t => t.Timestamp >= desde && t.EventType == "view_page")
            .GroupBy(t => t.Route)
            .Select(g => new { Route = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync();

        return rows.Select(r => new TopPagina(r.Route, r.Count)).ToList();
    }

    public async Task<List<DispositivoConteo>> GetDistribucionDispositivosAsync(int dias = 7)
    {
        var desde = DateTimeOffset.UtcNow.AddDays(-dias);

        var rows = await _context.TelemetryEvents
            .Where(t => t.Timestamp >= desde)
            .GroupBy(t => t.DeviceType ?? "Desconocido")
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return rows.Select(r => new DispositivoConteo(r.Label, r.Count)).ToList();
    }

    public async Task<List<LogEntry>> GetLogsRecientesAsync(int count = 8)
    {
        return await _context.Logs
            .Where(l => l.Level == "Error" || l.Level == "Critical" || l.Level == "Warning")
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync();
    }
}
