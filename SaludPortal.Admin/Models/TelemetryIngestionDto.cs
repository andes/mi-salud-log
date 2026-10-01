namespace SaludPortal.Admin.Models;

public record TelemetryIngestionDto(
    string EventType,
    string Route,
    string? Target,
    string? PatientId,
    string? SessionId,
    string? Source,
    string? MetadataJson,
    string? ClientIp,
    double? Latitude,
    double? Longitude,
    string? UserAgent,
    string? Browser,
    string? OsName,
    string? OsVersion,
    string? DeviceType);
