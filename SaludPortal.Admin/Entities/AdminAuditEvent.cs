using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaludPortal.Admin.Entities;

[Table("admin_audit_events")]
public class AdminAuditEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    [MaxLength(450)]
    [Column("user_id")]
    public string UserId { get; set; } = "";

    [Required]
    [MaxLength(256)]
    [Column("user_email")]
    public string UserEmail { get; set; } = "";

    [Required]
    [MaxLength(64)]
    [Column("action")]
    public string Action { get; set; } = "";

    [Column("cutoff_utc")]
    public DateTimeOffset? CutoffUtc { get; set; }

    [Column("deleted_logs")]
    public int DeletedLogs { get; set; }

    [Column("deleted_telemetry")]
    public int DeletedTelemetry { get; set; }
}
