using Microsoft.EntityFrameworkCore;
using SaludPortal.Admin.Data;
using SaludPortal.Admin.Entities;
using SaludPortal.Admin.Models;

namespace SaludPortal.Admin.Services;

public class LogsService
{
    private readonly ApplicationDbContext _context;

    public LogsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PageResult<LogEntry>> GetLogsAsync(
        int page,
        int pageSize,
        string? level = null,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        string? search = null)
    {
        IQueryable<LogEntry> query = _context.Logs;

        if (!string.IsNullOrWhiteSpace(level))
            query = query.Where(l => l.Level == level);

        if (dateFrom.HasValue)
            query = query.Where(l => l.Timestamp >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(l => l.Timestamp <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l =>
                EF.Functions.ILike(l.Message ?? "", $"%{search}%") ||
                EF.Functions.ILike(l.Category ?? "", $"%{search}%"));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<LogEntry> { Items = items, TotalCount = totalCount };
    }

    public async Task<LogEntry?> GetLogByIdAsync(long id)
        => await _context.Logs.FindAsync(id);

    public async Task<List<LogEntry>> GetAllForExportAsync(
        string? level = null,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        string? search = null,
        int maxRows = 1000)
    {
        maxRows = Math.Clamp(maxRows, 1, 50_000);

        IQueryable<LogEntry> query = _context.Logs;

        if (!string.IsNullOrWhiteSpace(level))
            query = query.Where(l => l.Level == level);

        if (dateFrom.HasValue)
            query = query.Where(l => l.Timestamp >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(l => l.Timestamp <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l =>
                EF.Functions.ILike(l.Message ?? "", $"%{search}%") ||
                EF.Functions.ILike(l.Category ?? "", $"%{search}%"));

        return await query
            .OrderByDescending(l => l.Timestamp)
            .Take(maxRows)
            .ToListAsync();
    }

    public async Task<List<HeatmapPoint>> GetHeatmapPointsAsync(
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        string? level = null)
    {
        IQueryable<LogEntry> query = _context.Logs
            .Where(l => l.Latitude.HasValue && l.Longitude.HasValue);

        if (!string.IsNullOrWhiteSpace(level))
            query = query.Where(l => l.Level == level);

        if (dateFrom.HasValue)
            query = query.Where(l => l.Timestamp >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(l => l.Timestamp <= dateTo.Value);

        var rawPoints = await query
            .Select(l => new { l.Latitude, l.Longitude })
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
