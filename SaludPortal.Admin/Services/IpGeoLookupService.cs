using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SaludPortal.Admin.Data;
using SaludPortal.Admin.Entities;

namespace SaludPortal.Admin.Services;

public class IpGeoLookupService
{
    private readonly ApplicationDbContext _db;

    public IpGeoLookupService(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Resolves the final lat/lon for the given IP.
    /// - If both coordinates are already provided, caches them and returns them.
    /// - If one or both are missing, checks the DB cache first; on a miss returns null.
    /// Always falls back silently — ingestion must never fail because of a geo lookup error.
    /// </summary>
    public async Task<(double? Lat, double? Lon)> ResolveAsync(string? ip, double? providedLat, double? providedLon)
    {
        if (string.IsNullOrWhiteSpace(ip) || IsPrivateOrLoopback(ip))
            return (providedLat, providedLon);

        try
        {
            var cached = await _db.IpGeoCache.FindAsync(ip);

            // Both coordinates provided by the client → cache them and return immediately.
            if (providedLat is not null && providedLon is not null)
            {
                await UpsertCacheAsync(ip, providedLat, providedLon, cached);
                return (providedLat, providedLon);
            }

            // At least one coordinate missing → try the DB cache first.
            if (cached is not null)
                return (providedLat ?? cached.Latitude, providedLon ?? cached.Longitude);

            // Cache miss or expired → return null.
            return (null, null);
        }
        catch
        {
            return (providedLat, providedLon);
        }
    }

    private async Task UpsertCacheAsync(string ip, double? lat, double? lon, IpGeoCache? existing)
    {
        IpGeoCache entity;
        if (existing is null)
        {
            entity = new IpGeoCache { Ip = ip, Latitude = lat, Longitude = lon, FetchedAt = DateTimeOffset.UtcNow };
            _db.IpGeoCache.Add(entity);
        }
        else
        {
            entity = existing;
            entity.Latitude = lat;
            entity.Longitude = lon;
            entity.FetchedAt = DateTimeOffset.UtcNow;
        }

        try { await _db.SaveChangesAsync(); }
        catch
        {
            // Concurrent upsert for the same IP — detach so the caller's
            // SaveChangesAsync (same DbContext scope) is not affected.
            _db.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        }
    }

    private static bool IsPrivateOrLoopback(string ip)
    {
        if (!IPAddress.TryParse(ip, out var address))
            return true;

        if (IPAddress.IsLoopback(address))
            return true;

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            return address.IsIPv6LinkLocal || address.IsIPv6SiteLocal;

        var bytes = address.GetAddressBytes();
        return bytes[0] == 10
            || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            || (bytes[0] == 192 && bytes[1] == 168);
    }

    private record IpGuideResponse(IpGuideLocation? Location);
    private record IpGuideLocation(double? Latitude, double? Longitude);
}
