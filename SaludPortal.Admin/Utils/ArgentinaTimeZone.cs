namespace SaludPortal.Admin.Utils;

/// <summary>
/// Zona horaria de Argentina (UTC-3) para cortes de calendario en el Admin.
/// </summary>
public static class ArgentinaTimeZone
{
    private static readonly TimeZoneInfo Zone;

    static ArgentinaTimeZone()
    {
        try
        {
            Zone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                Zone = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
            }
            catch
            {
                Zone = TimeZoneInfo.CreateCustomTimeZone(
                    "Argentina Standard Time",
                    TimeSpan.FromHours(-3),
                    "Argentina Standard Time",
                    "Argentina Standard Time");
            }
        }
    }

    /// <summary>
    /// Inicio del día calendario actual en Argentina, como instante UTC
    /// (comparable con timestamps almacenados en UTC).
    /// </summary>
    public static DateTimeOffset StartOfTodayUtc()
    {
        var nowArt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);
        var startOfDayArt = DateTime.SpecifyKind(nowArt.Date, DateTimeKind.Unspecified);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDayArt, Zone);
        return new DateTimeOffset(startUtc, TimeSpan.Zero);
    }
}
