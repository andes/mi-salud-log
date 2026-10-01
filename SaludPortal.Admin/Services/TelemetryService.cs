using Microsoft.EntityFrameworkCore;
using SaludPortal.Admin.Data;
using SaludPortal.Admin.Entities;
using SaludPortal.Admin.Models;

namespace SaludPortal.Admin.Services;

public class TelemetryService
{
    private readonly ApplicationDbContext _context;

    public TelemetryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PageResult<TelemetryEvent>> GetEventsAsync(
        int page,
        int pageSize,
        string? eventType = null,
        string? patientId = null,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        string? search = null)
    {
        IQueryable<TelemetryEvent> query = _context.TelemetryEvents;

        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(t => t.EventType == eventType);

        if (!string.IsNullOrWhiteSpace(patientId))
            query = query.Where(t => t.PatientId == patientId);

        if (dateFrom.HasValue)
            query = query.Where(t => t.Timestamp >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(t => t.Timestamp <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t =>
                EF.Functions.ILike(t.Route, $"%{search}%") ||
                EF.Functions.ILike(t.Target ?? "", $"%{search}%") ||
                EF.Functions.ILike(t.MetadataJson ?? "", $"%{search}%") ||
                EF.Functions.ILike(t.Source ?? "", $"%{search}%"));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<TelemetryEvent> { Items = items, TotalCount = totalCount };
    }

    public async Task<TelemetryEvent?> GetEventByIdAsync(long id)
        => await _context.TelemetryEvents.FindAsync(id);

    public async Task<List<TelemetryEvent>> GetAllForExportAsync(
        string? eventType = null,
        string? patientId = null,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        string? search = null,
        int maxRows = 1000)
    {
        maxRows = Math.Clamp(maxRows, 1, 50_000);

        IQueryable<TelemetryEvent> query = _context.TelemetryEvents;

        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(t => t.EventType == eventType);

        if (!string.IsNullOrWhiteSpace(patientId))
            query = query.Where(t => t.PatientId == patientId);

        if (dateFrom.HasValue)
            query = query.Where(t => t.Timestamp >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(t => t.Timestamp <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t =>
                EF.Functions.ILike(t.Route, $"%{search}%") ||
                EF.Functions.ILike(t.Target ?? "", $"%{search}%") ||
                EF.Functions.ILike(t.MetadataJson ?? "", $"%{search}%") ||
                EF.Functions.ILike(t.Source ?? "", $"%{search}%"));

        return await query
            .OrderByDescending(t => t.Timestamp)
            .Take(maxRows)
            .ToListAsync();
    }

    public async Task<List<HeatmapPoint>> GetHeatmapPointsAsync(
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        string? eventType = null)
    {
        IQueryable<TelemetryEvent> query = _context.TelemetryEvents
            .Where(t => t.Latitude.HasValue && t.Longitude.HasValue);

        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(t => t.EventType == eventType);

        if (dateFrom.HasValue)
            query = query.Where(t => t.Timestamp >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(t => t.Timestamp <= dateTo.Value);

        var rawPoints = await query
            .Select(t => new { t.Latitude, t.Longitude })
            .Take(50_000)
            .ToListAsync();

        return rawPoints
            .GroupBy(p => new
            {
                Lat = Math.Round(p.Latitude!.Value, 3),
                Lon = Math.Round(p.Longitude!.Value, 3)
            })
            .Select(g => new HeatmapPoint(g.Key.Lat, g.Key.Lon, g.Count()))
            .OrderByDescending(p => p.Intensity)
            .Take(5000)
            .ToList();
    }
}