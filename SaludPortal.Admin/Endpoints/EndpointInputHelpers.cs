namespace SaludPortal.Admin.Endpoints;

using System.Security.Cryptography;
using System.Text;

internal static class EndpointInputHelpers
{
    public static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    public static string? NormalizeMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return null;

        var trimmed = metadataJson.Trim();
        return trimmed.Length <= 4000 ? trimmed : null;
    }

    public static string? ResolveClientIp(HttpContext ctx)
    {
        var fromForwardedHeader = ctx.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(fromForwardedHeader))
        {
            var first = fromForwardedHeader.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(first))
                return first;
        }

        return ctx.Connection.RemoteIpAddress?.ToString();
    }

    public static bool IsPayloadTooLarge(HttpContext ctx, int maxPayloadBytes)
    {
        if (maxPayloadBytes <= 0)
            return false;

        return ctx.Request.ContentLength.HasValue && ctx.Request.ContentLength.Value > maxPayloadBytes;
    }

    public static bool IsValidApiKey(HttpContext ctx, string? expectedKey)
    {
        if (string.IsNullOrWhiteSpace(expectedKey))
            return false;

        var providedKey = ctx.Request.Headers["X-Api-Key"].ToString();
        if (string.IsNullOrWhiteSpace(providedKey))
            return false;

        var expectedBytes = Encoding.UTF8.GetBytes(expectedKey);
        var providedBytes = Encoding.UTF8.GetBytes(providedKey);
        return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    public static bool IsAllowedTelemetryType(string eventType, string[] allowedTypes)
    {
        if (allowedTypes.Length == 0)
            return true;

        return allowedTypes.Any(type => string.Equals(type, eventType, StringComparison.OrdinalIgnoreCase));
    }
}
