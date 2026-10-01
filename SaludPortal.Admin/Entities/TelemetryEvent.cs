using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaludPortal.Admin.Entities;

[Table("telemetry_events")]
public class TelemetryEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    [MaxLength(64)]
    [Column("event_type")]
    public string EventType { get; set; } = "";

    [Required]
    [MaxLength(512)]
    [Column("route")]
    public string Route { get; set; } = "";

    [MaxLength(512)]
    [Column("target")]
    public string? Target { get; set; }

    [MaxLength(128)]
    [Column("patient_id")]
    public string? PatientId { get; set; }

    [MaxLength(128)]
    [Column("session_id")]
    public string? SessionId { get; set; }

    [MaxLength(64)]
    [Column("source")]
    public string? Source { get; set; }

    [Column("metadata_json")]
    public string? MetadataJson { get; set; }

    [MaxLength(64)]
    [Column("client_ip")]
    public string? ClientIp { get; set; }

    [Column("latitude")]
    public double? Latitude { get; set; }

    [Column("longitude")]
    public double? Longitude { get; set; }

    [MaxLength(1024)]
    [Column("user_agent")]
    public string? UserAgent { get; set; }

    [MaxLength(64)]
    [Column("browser")]
    public string? Browser { get; set; }

    [MaxLength(64)]
    [Column("os_name")]
    public string? OsName { get; set; }

    [MaxLength(32)]
    [Column("os_version")]
    public string? OsVersion { get; set; }

    [MaxLength(32)]
    [Column("device_type")]
    public string? DeviceType { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }
}