using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaludPortal.Admin.Entities;

[Table("logs")]
public class LogEntry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    [MaxLength(16)]
    [Column("level")]
    public string Level { get; set; } = "";

    [MaxLength(512)]
    [Column("category")]
    public string? Category { get; set; }

    [Column("message")]
    public string? Message { get; set; }

    [Column("exception")]
    public string? Exception { get; set; }

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

    [MaxLength(128)]
    [Column("patient_id")]
    public string? PatientId { get; set; }

    [MaxLength(128)]
    [Column("session_id")]
    public string? SessionId { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }
}