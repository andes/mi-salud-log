using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaludPortal.Admin.Entities;

[Table("ip_geo_cache")]
public class IpGeoCache
{
    [Key]
    [MaxLength(64)]
    [Column("ip")]
    public string Ip { get; set; } = null!;

    [Column("latitude")]
    public double? Latitude { get; set; }

    [Column("longitude")]
    public double? Longitude { get; set; }

    [Column("fetched_at")]
    public DateTimeOffset FetchedAt { get; set; }
}
