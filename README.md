# SaludPortal.Admin — Panel de administración

Panel de administración (Blazor Server + Minimal API, MudBlazor v9) para consultar logs de aplicación y telemetría de uso del portal de pacientes.

## Proyecto


| Proyecto            | Rol                                                           |
| ------------------- | ------------------------------------------------------------- |
| `SaludPortal.Admin` | UI MudBlazor + API de ingesta (`/api/logs`, `/api/telemetry`) |


Solución: `[SaludPortal.Admin.slnx](SaludPortal.Admin.slnx)`.

Requiere **PostgreSQL** (`ConnectionStrings:LogsDb`). Las migraciones EF Core se aplican al
iniciar la aplicación.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- PostgreSQL 17 (o compatible)
- Docker (opcional)



## Base de datos (Development)

```bash
docker run -d --name salud-portal-db \
  -e POSTGRES_DB=salud_logs \
  -e POSTGRES_USER=salud \
  -e POSTGRES_PASSWORD=salud_pass \
  -p 5432:5432 \
  -v postgres_data:/var/lib/postgresql/data \
  postgres:17-alpine
```

Connection string de desarrollo (coincide con `appsettings.Development.json`):

```
Host=localhost;Port=5432;Database=salud_logs;Username=salud;Password=salud_pass
```

Si el contenedor ya existe: `docker start salud-portal-db`.

## Usuarios administradores (Development)

`appsettings.Development.json` no trae usuarios. El primer administrador se carga con
**User Secrets** (se leen automáticamente en `Development`):

```powershell
cd SaludPortal.Admin
dotnet user-secrets set "AdminUsers:0:Email" "<email>"
dotnet user-secrets set "AdminUsers:0:Password" "<password>"
```

Los usuarios solo se crean si no existen en la base; los existentes conservan su contraseña.
El resto de los administradores se gestiona desde el panel.

## Build y ejecución

```bash
dotnet build SaludPortal.Admin.slnx
dotnet run --project SaludPortal.Admin
```

URL típica en Development: `https://localhost:7180`.

### Docker

```bash
docker build -t salud-admin .
docker run -d -p 8081:8080 \
  -e ConnectionStrings__LogsDb="Host=<host>;Port=5432;Database=salud_logs;Username=<user>;Password=<pass>" \
  -e LogIngestion__ApiKey="<api-key>" \
  -e AdminUsers__0__Email="<email>" \
  -e AdminUsers__0__Password="<password>" \
  salud-admin
```

`AdminUsers__0__*` es necesario en el primer arranque contra una base vacía: sin al menos un
usuario no se puede iniciar sesión. `LogIngestion__ApiKey` debe coincidir con `AdminLogs__ApiKey`
del portal Web.



### Migraciones

```bash
dotnet ef migrations add <Nombre> --project SaludPortal.Admin
```



## Configuración relevante


| Sección                                    | Propósito                                                    |
| ------------------------------------------ | ------------------------------------------------------------ |
| `ConnectionStrings:LogsDb`                 | PostgreSQL                                                   |
| `AdminUsers`                               | Usuarios Identity sembrados al arrancar                      |
| `LogIngestion:ApiKey`                      | Clave que deben enviar los clientes de ingesta (`X-Api-Key`) |
| `LogIngestion:RateLimit:*`                 | Límite de peticiones de ingesta                              |
| `LogIngestion:Telemetry:AllowedEventTypes` | Tipos de evento de telemetría aceptados                      |




## Autenticación

ASP.NET Identity (cookie). Login por formulario HTML POST a `/account/login`. Usuarios iniciales
desde `AdminUsers` en configuración.

## Health

```
GET /health
```



## Ingesta desde el portal Web

El portal (otro repositorio) puede enviar logs y telemetría si configura la misma API key:


| En el Admin             | En el Web           |
| ----------------------- | ------------------- |
| `LogIngestion:ApiKey`   | `AdminLogs:ApiKey`  |
| (URL pública del Admin) | `AdminLogs:BaseUrl` |


Endpoints:

- `POST /api/logs` — requiere `X-Api-Key`
- `POST /api/telemetry` — requiere `X-Api-Key`

En Development, Scalar (documentación OpenAPI interactiva): `https://localhost:7180/scalar/v1`.

## Documentación

- Manual de uso del panel: [docs/manual-admin.md](docs/manual-admin.md)
- Pipeline técnico logs/telemetría: [docs/telemetria-y-logs.md](docs/telemetria-y-logs.md)

