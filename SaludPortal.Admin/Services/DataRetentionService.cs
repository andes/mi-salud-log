using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaludPortal.Admin.Data;
using SaludPortal.Admin.Entities;

namespace SaludPortal.Admin.Services;

public record PurgePreview(int LogCount, int TelemetryCount);

public record PurgeResult(bool Succeeded, string? Error, int DeletedLogs, int DeletedTelemetry);

public class DataRetentionService
{
    public const string PurgeDataAction = "purge_data";

    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public DataRetentionService(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<PurgePreview> GetPurgePreviewAsync(DateTimeOffset cutoff, CancellationToken ct = default)
    {
        var logCount = await _db.Logs.CountAsync(l => l.Timestamp < cutoff, ct);
        var telemetryCount = await _db.TelemetryEvents.CountAsync(t => t.Timestamp < cutoff, ct);
        return new PurgePreview(logCount, telemetryCount);
    }

    public async Task<PurgeResult> PurgeAsync(
        DateTimeOffset cutoff,
        bool purgeLogs,
        bool purgeTelemetry,
        string password,
        ClaimsPrincipal principal,
        CancellationToken ct = default)
    {
        if (!purgeLogs && !purgeTelemetry)
            return new PurgeResult(false, "Seleccioná al menos logs o telemetría.", 0, 0);

        if (string.IsNullOrWhiteSpace(password))
            return new PurgeResult(false, "Ingresá tu contraseña para confirmar.", 0, 0);

        if (cutoff >= DateTimeOffset.UtcNow)
            return new PurgeResult(false, "La fecha debe ser anterior al momento actual.", 0, 0);

        var user = await _userManager.GetUserAsync(principal);
        if (user is null)
            return new PurgeResult(false, "No se pudo identificar al usuario actual.", 0, 0);

        var passwordOk = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordOk)
            return new PurgeResult(false, "Contraseña incorrecta.", 0, 0);

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var deletedLogs = 0;
        var deletedTelemetry = 0;

        var deletedAt = DateTimeOffset.UtcNow;

        if (purgeLogs)
            deletedLogs = await _db.Logs
                .Where(l => l.Timestamp < cutoff)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(l => l.IsDeleted, true)
                    .SetProperty(l => l.DeletedAt, deletedAt), ct);

        if (purgeTelemetry)
            deletedTelemetry = await _db.TelemetryEvents
                .Where(t => t.Timestamp < cutoff)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.IsDeleted, true)
                    .SetProperty(t => t.DeletedAt, deletedAt), ct);

        _db.AdminAuditEvents.Add(new AdminAuditEvent
        {
            Timestamp = DateTimeOffset.UtcNow,
            UserId = user.Id,
            UserEmail = user.Email ?? user.UserName ?? "",
            Action = PurgeDataAction,
            CutoffUtc = cutoff,
            DeletedLogs = deletedLogs,
            DeletedTelemetry = deletedTelemetry
        });
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new PurgeResult(true, null, deletedLogs, deletedTelemetry);
    }

    public Task<List<AdminAuditEvent>> GetRecentAuditEventsAsync(int limit = 50, CancellationToken ct = default)
        => _db.AdminAuditEvents
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync(ct);
}
