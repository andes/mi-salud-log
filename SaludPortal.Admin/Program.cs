using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Scalar.AspNetCore;
using PdfSharp.Fonts;
using System.Threading.RateLimiting;
using SaludPortal.Admin.Components;
using SaludPortal.Admin.Data;
using SaludPortal.Admin.Endpoints;
using SaludPortal.Admin.Services;
using SaludPortal.Admin.Services.ExportFiles;
using MudBlazor.Services;

// Configure PDFsharp font resolution before any PDF generation.
// On Windows, GDI-backed fonts are used. On Linux (Docker), fonts-liberation must be
// installed in the container (see Dockerfile.admin).
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    GlobalFontSettings.UseWindowsFontsUnderWindows = true;

var builder = WebApplication.CreateBuilder(args);

// EF Core + Identity (only manages Identity tables; logs table is queried via raw Npgsql)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LogsDb")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/";
    options.AccessDeniedPath = "/";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "SaludPortal Admin API";
        document.Info.Version = "v1";
        document.Info.Description = "Internal APIs for authentication, logs ingestion and telemetry ingestion.";
        return Task.CompletedTask;
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<LogsService>();
builder.Services.AddScoped<TelemetryService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<UsuariosService>();
builder.Services.AddScoped<DataRetentionService>();
builder.Services.AddScoped<IpGeoLookupService>();
builder.Services.AddTransient<ExportCsvService>();
builder.Services.AddTransient<ExportJsonService>();
builder.Services.AddTransient<ExportPdfService>();

var ingestPermitLimit = builder.Configuration.GetValue<int?>("LogIngestion:RateLimit:PermitLimit") ?? 120;
var ingestWindowSeconds = builder.Configuration.GetValue<int?>("LogIngestion:RateLimit:WindowSeconds") ?? 60;
var ingestQueueLimit = builder.Configuration.GetValue<int?>("LogIngestion:RateLimit:QueueLimit") ?? 0;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("ingestion", limiter =>
    {
        limiter.PermitLimit = ingestPermitLimit;
        limiter.Window = TimeSpan.FromSeconds(ingestWindowSeconds);
        limiter.QueueLimit = ingestQueueLimit;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 4500;
    config.SnackbarConfiguration.HideTransitionDuration = 150;
    config.SnackbarConfiguration.ShowTransitionDuration = 150;
});

var app = builder.Build();

// Apply pending EF migrations and seed admin user on startup.
// On a brand-new DB, Npgsql EF creates __EFMigrationsHistory automatically before querying it.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await AdminSeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("SaludPortal Admin API"));
}

app.MapAccountEndpoints();
app.MapLogsEndpoints();
app.MapTelemetryEndpoints();
app.MapExportEndpoints();

app.MapGet("/health", () => Results.Ok());
app.Run();
