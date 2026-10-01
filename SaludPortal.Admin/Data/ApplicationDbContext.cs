using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaludPortal.Admin.Entities;

namespace SaludPortal.Admin.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<LogEntry> Logs => Set<LogEntry>();
    public DbSet<TelemetryEvent> TelemetryEvents => Set<TelemetryEvent>();
    public DbSet<IpGeoCache> IpGeoCache => Set<IpGeoCache>();
    public DbSet<AdminAuditEvent> AdminAuditEvents => Set<AdminAuditEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<LogEntry>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Timestamp)
                .HasDefaultValueSql("NOW()")
                .ValueGeneratedOnAdd();
            e.Property(l => l.IsDeleted)
                .HasDefaultValue(false);
            e.HasQueryFilter(l => !l.IsDeleted);

            e.HasIndex(l => l.Timestamp);
            e.HasIndex(l => l.PatientId);
            e.HasIndex(l => l.ClientIp);
            e.HasIndex(l => new { l.Level, l.Timestamp }).HasDatabaseName("IX_logs_level_timestamp");
            e.HasIndex(l => new { l.Timestamp, l.Latitude, l.Longitude }).HasDatabaseName("IX_logs_timestamp_lat_lon");
            e.HasIndex(l => new { l.IsDeleted, l.Timestamp }).HasDatabaseName("IX_logs_is_deleted_timestamp");
        });

        builder.Entity<TelemetryEvent>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Timestamp)
                .HasDefaultValueSql("NOW()")
                .ValueGeneratedOnAdd();
            e.Property(t => t.IsDeleted)
                .HasDefaultValue(false);
            e.HasQueryFilter(t => !t.IsDeleted);

            e.HasIndex(t => t.Timestamp);
            e.HasIndex(t => t.EventType);
            e.HasIndex(t => t.PatientId);
            e.HasIndex(t => t.ClientIp);
            e.HasIndex(t => new { t.EventType, t.Timestamp }).HasDatabaseName("IX_telemetry_event_type_timestamp");
            e.HasIndex(t => t.DeviceType).HasDatabaseName("IX_telemetry_device_type");
            e.HasIndex(t => new { t.Timestamp, t.Latitude, t.Longitude }).HasDatabaseName("IX_telemetry_timestamp_lat_lon");
            e.HasIndex(t => new { t.IsDeleted, t.Timestamp }).HasDatabaseName("IX_telemetry_is_deleted_timestamp");
        });

        builder.Entity<IpGeoCache>(e =>
        {
            e.HasKey(g => g.Ip);
            e.HasIndex(g => g.FetchedAt);
        });

        builder.Entity<AdminAuditEvent>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Timestamp)
                .HasDefaultValueSql("NOW()")
                .ValueGeneratedOnAdd();

            e.HasIndex(a => a.Timestamp);
        });
    }
}
