namespace SaludPortal.Admin.Models;

public record LogIngestionDto(
    string Level,
    string? Category,
    string? Message,
    string? Exception,
    string? ClientIp,
    double? Latitude,
    double? Longitude,
    string? UserAgent,
    string? Browser,
    string? OsName,
    string? OsVersion,
    string? DeviceType,
    string? PatientId,
    string? SessionId);
