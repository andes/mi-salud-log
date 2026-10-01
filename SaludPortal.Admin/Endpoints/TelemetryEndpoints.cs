using Microsoft.OpenApi.Models;
using SaludPortal.Admin.Data;
using SaludPortal.Admin.Entities;
using SaludPortal.Admin.Models;
using SaludPortal.Admin.Services;

namespace SaludPortal.Admin.Endpoints;

public static class TelemetryEndpoints
{
    public static void MapTelemetryEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/telemetry", async (
            HttpContext ctx,
            IConfiguration config,
            ApplicationDbContext db,
            IpGeoLookupService geoLookup) =>
        {
            var expectedKey = config["LogIngestion:ApiKey"];
            if (!EndpointInputHelpers.IsValidApiKey(ctx, expectedKey))
                return Results.Unauthorized();

            var maxPayloadBytes = config.GetValue<int?>("LogIngestion:MaxPayloadBytes") ?? 32768;
            if (EndpointInputHelpers.IsPayloadTooLarge(ctx, maxPayloadBytes))
                return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);

            if (!ctx.Request.HasJsonContentType())
                return Results.BadRequest();

            var dto = await ctx.Request.ReadFromJsonAsync<TelemetryIngestionDto>();
            if (dto is null || string.IsNullOrWhiteSpace(dto.EventType) || string.IsNullOrWhiteSpace(dto.Route))
                return Results.BadRequest();

            var eventType = dto.EventType.Trim();
            var allowedTypes = config.GetSection("LogIngestion:Telemetry:AllowedEventTypes").Get<string[]>() ?? [];
            if (!EndpointInputHelpers.IsAllowedTelemetryType(eventType, allowedTypes))
                return Results.BadRequest();

            var metadataJson = EndpointInputHelpers.NormalizeMetadata(dto.MetadataJson);
            if (metadataJson is null && !string.IsNullOrWhiteSpace(dto.MetadataJson))
                return Results.BadRequest();

            var clientIp = EndpointInputHelpers.Normalize(dto.ClientIp, 64) ?? EndpointInputHelpers.ResolveClientIp(ctx);

            var (resolvedLat, resolvedLon) = await geoLookup.ResolveAsync(clientIp, dto.Latitude, dto.Longitude);

            db.TelemetryEvents.Add(new TelemetryEvent
            {
                EventType = eventType,
                Route = dto.Route.Trim(),
                Target = EndpointInputHelpers.Normalize(dto.Target, 512),
                PatientId = EndpointInputHelpers.Normalize(dto.PatientId, 128),
                SessionId = EndpointInputHelpers.Normalize(dto.SessionId, 128),
                Source = EndpointInputHelpers.Normalize(dto.Source, 64),
                MetadataJson = metadataJson,
                ClientIp = clientIp,
                Latitude = resolvedLat,
                Longitude = resolvedLon,
                UserAgent = EndpointInputHelpers.Normalize(dto.UserAgent, 1024),
                Browser = EndpointInputHelpers.Normalize(dto.Browser, 64),
                OsName = EndpointInputHelpers.Normalize(dto.OsName, 64),
                OsVersion = EndpointInputHelpers.Normalize(dto.OsVersion, 32),
                DeviceType = EndpointInputHelpers.Normalize(dto.DeviceType, 32)
            });

            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .AllowAnonymous()
        .RequireRateLimiting("ingestion")
        .DisableAntiforgery()
        .WithName("IngestTelemetry")
        .WithSummary("Ingest telemetry event")
        .WithDescription("Persists one telemetry event in the admin database. Requires X-Api-Key header and enforces ingestion rate limits.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status413PayloadTooLarge)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status429TooManyRequests)
        .WithOpenApi(operation =>
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Api-Key",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Shared API key for ingestion endpoints.",
                Schema = new OpenApiSchema { Type = "string" }
            });
            return operation;
        });
    }
}
