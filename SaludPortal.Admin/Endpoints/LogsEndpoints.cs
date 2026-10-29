using Microsoft.OpenApi;
using SaludPortal.Admin.Data;
using SaludPortal.Admin.Entities;
using SaludPortal.Admin.Models;
using SaludPortal.Admin.Services;

namespace SaludPortal.Admin.Endpoints;

public static class LogsEndpoints
{
    public static void MapLogsEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/logs", async (
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

            var dto = await ctx.Request.ReadFromJsonAsync<LogIngestionDto>();
            if (dto is null || string.IsNullOrWhiteSpace(dto.Level))
                return Results.BadRequest();

            var clientIp = EndpointInputHelpers.Normalize(dto.ClientIp, 64) ?? EndpointInputHelpers.ResolveClientIp(ctx);
            
            var (resolvedLat, resolvedLon) = await geoLookup.ResolveAsync(clientIp, dto.Latitude, dto.Longitude);

            var entry = new LogEntry
            {
                Level       = EndpointInputHelpers.Normalize(dto.Level, 16) ?? dto.Level,
                Category    = dto.Category,
                Message     = dto.Message,
                Exception   = dto.Exception,
                ClientIp    = clientIp,
                Latitude    = resolvedLat,
                Longitude   = resolvedLon,
                UserAgent   = EndpointInputHelpers.Normalize(dto.UserAgent, 1024),
                Browser     = EndpointInputHelpers.Normalize(dto.Browser, 64),
                OsName      = EndpointInputHelpers.Normalize(dto.OsName, 64),
                OsVersion   = EndpointInputHelpers.Normalize(dto.OsVersion, 32),
                DeviceType  = EndpointInputHelpers.Normalize(dto.DeviceType, 32),
                PatientId   = EndpointInputHelpers.Normalize(dto.PatientId, 128),
                SessionId   = EndpointInputHelpers.Normalize(dto.SessionId, 128),
            };

            db.Logs.Add(entry);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .AllowAnonymous()
        .RequireRateLimiting("ingestion")
        .DisableAntiforgery()
        .WithName("IngestLogs")
        .WithSummary("Ingest application log entry")
        .WithDescription("Persists one log event in the admin database. Requires X-Api-Key header and enforces ingestion rate limits.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status413PayloadTooLarge)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status429TooManyRequests)
        .AddOpenApiOperationTransformer((operation, _, _) =>
        {
            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Api-Key",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Shared API key for ingestion endpoints.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });
            return Task.CompletedTask;
        });
    }
}
