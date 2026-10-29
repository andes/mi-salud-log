# Especificación del Sistemas de Administración de Logs y Telemetría

## Portal MiSalud

Fecha: 19/05/2026

Actualizacion: 09/10/2026

## 1- Propósito

Definir los requisitos funcionales, no funcionales, de arquitectura, de datos y de operación para el sistema de administración de logs y telemetría del sitio Portal MiSalud.
Este documento formaliza el alcance del proyecto y deja una base para extender capacidades en futuras versiones.

## 2- Contexto y alcance

Portal MiSalud es una solución existente desarrollada en .NET Blazor. El sistema de observabilidad asociado contempla tres componentes principales:

1. Sitio web administrativo para visualización y gestión.
2. API de ingesta para recibir eventos desde Portal MiSalud.
3. Base de datos PostgresSQL para persistencia de logs y telemetría.



### Alcance incluido

1 Ingesta de logs de aplicación.
2 Ingesta de eventos de telemetría.
3 Visualización de logs en panel administrativo.
4 Visualización de telemetría en panel administrativo.
5 Visualización del recorrido de un usuario en el portal durante su sesión.
6 Gestión de usuarios administradores (alta, edición, eliminación).
7 Autenticación con [ASP.NET](http://ASP.NET) Identity.

## 3- Actores

1. Administrador: usuario autenticado que consulta logs/telemetría y administra usuarios.
2. Portal MiSalud: sistema emisor de logs y eventos.
3. API de ingesta: servicio backend que valida y persiste eventos.
4. PostgresSQL: almacenamiento principal.



## 4- Arquitectura lógica

```text
┌──────────────────────────────────────────────────────────────────────────┐
│                              PORTAL MISALUD                              │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │ Eventos de Usuario (login, navegación, descargas, etc.)            │  │
│  │ Eventos de Error (excepciones, fallos API, etc.)                   │  │
│  └────────────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────┬─────────────────────────────────────┘
                                     │
                                     ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                                 API REST                                 │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │ Recepción de Logs y Telemetría (expuesta por el dashboard)         │  │
│  └────────────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────┬─────────────────────────────────────┘
                                     │
                                     ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                          BASE DE DATOS RELACIONAL                        │
│  ┌─────────────────────────────────┐  ┌───────────────────────────────┐  │
│  │ logs                            │  │ telemetryevents               │  │
│  │ (logs de Portal)                │  │ (eventos de telemetría)       │  │
│  └─────────────────────────────────┘  └───────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │ AspNetUsers                                                        │  │
│  │ (usuarios propios del sistema de dashboards, login independiente)  │  │
│  └────────────────────────────────────────────────────────────────────┘  │
│  ┌─────────────────────────────────┐  ┌───────────────────────────────┐  │
│  │ AspNetUserRoles                 │  │ AspNetRoles                   │  │
│  └─────────────────────────────────┘  └───────────────────────────────┘  │
│  ┌─────────────────────────────────┐  ┌───────────────────────────────┐  │
│  │ AspNetRoleClaims                │  │ AspNetUserClaims              │  │
│  └─────────────────────────────────┘  └───────────────────────────────┘  │
│  ┌─────────────────────────────────┐  ┌───────────────────────────────┐  │
│  │ AspNetUserLogins                │  │ AspNetUserTokens              │  │
│  └─────────────────────────────────┘  └───────────────────────────────┘  │
└────────────────────────────────────┬─────────────────────────────────────┘
                                     │
                                     ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                       PANELES DE CONTROL (DASHBOARDS)                    │
│  ┌─────────────────────────────────┐  ┌───────────────────────────────┐  │
│  │ Dashboard de Usuarios           │  │ Dashboard de Errores          │  │
│  │ (visualiza logs de usuario      │  │ (visualiza logs de error y    │  │
│  │  de MiSalud)                    │  │  gestiona incidencias)        │  │
│  └─────────────────────────────────┘  └───────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │ Administración de usuarios admin                                   │  │
│  │ (visualiza usuarios propios del sistema de dashboards)             │  │
│  └────────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────────┘
```

Flujo principal:

1. Portal MiSalud genera evento (log o telemetría).
2. Portal MiSalud envía evento por HTTP POST a la API del sistema admin.
3. API valida API key, estructura y contenido del payload.
4. API normaliza campos y persiste en PostgreSQL.
5. Sitio admin consulta y muestra información paginada y filtrable.

Flujo de contingencia por falta de conexión:

1. Si Portal MiSalud no tiene conexión con la API, el evento se almacena temporalmente en el navegador.
2. Al recuperar conectividad, Portal MiSalud reintenta el envío de los eventos pendientes.
3. Los eventos pendientes deben enviarse manteniendo trazabilidad por sessionid y patientid cuando corresponda.



## 5- Gestión de sesión y correlación

Portal MiSalud se integra con una API externa de terceros, pero actualmente no se dispone de esa integración para identificación de sesión.
Decisión vigente:

1. El portal genera un sessionid propio.
2. sessionid se envía en logs y telemetría para trazabilidad.
3. patientid se utiliza como identificador de paciente cuando está disponible.



### 5.1 Recorrido del usuario en la sesión

A partir del sessionid, el panel administrativo reconstruye el recorrido que realizó un usuario en Portal MiSalud durante una sesión:

1. Se consideran todos los eventos de telemetría (`view_page`, `click`, `download_pdf`, `view_image`, `copy_link`, etc.) y los logs que comparten el mismo sessionid.
2. Los eventos se presentan en una línea de tiempo ordenada cronológicamente por createdat, mostrando para cada paso: hora, tipo de evento, route, target y, si existe, metadata.
3. Los logs de nivel Warning o superior asociados a la sesión se intercalan en la línea de tiempo y se destacan visualmente, para identificar en qué paso del recorrido ocurrió un error.
4. Se muestra un resumen de la sesión: patientid (si está disponible), inicio, fin y duración (primer y último evento), cantidad de páginas visitadas, cantidad de acciones y de errores, y contexto cliente (navegador, sistema operativo, tipo de dispositivo, IP).
5. Se accede al recorrido desde el detalle de un evento de telemetría o de un log (enlace por sessionid) y desde un listado de sesiones filtrable por paciente y rango temporal.
6. Si el paciente se autentica en mitad de la sesión, los eventos previos sin patientid se muestran igualmente como parte del mismo recorrido.



## 6 Interfaces de ingesta



### 6.1 Endpoint de logs

Ruta: ***/api/logs***
Método: POST
Autenticación: Header X-Api-Key
Respuestas:

1. 204 NoContent: evento persistido.
2. 400 BadRequest: payload inválido.
3. 401 Unauthorized: API key inválida o ausente.



### 6.2 Endpoint de telemetría

Ruta: ***/api/telemetry***
Método: POST
Autenticación: Header X-Api-Key
Respuestas:

1. 204 NoContent: evento persistido.
2. 400 BadRequest: payload inválido.
3. 401 Unauthorized: API key inválida o ausente.



## 7 Modelo de datos



### 7.1 Tabla logs

Campos y tipos:

1. id: bigint, PK, identity.
2. level: character varying(16), NOT NULL.
3. category: character varying(512), NULL.
4. message: text, NULL.
5. exception: text, NULL.
6. browser: character varying(64), NULL.
7. clientip: character varying(64), NULL.
8. devicetype: character varying(32), NULL.
9. latitude: double precision, NULL.
10. longitude: double precision, NULL.
11. osname: character varying(64), NULL.
12. osversion: character varying(32), NULL.
13. patientid: character varying(128), NULL.
14. sessionid: character varying(128), NULL.
15. useragent: character varying(1024), NULL.
16. resolverat: timestamp with time zone, NULL
17. resolutionnotes: character varying(1024), NULL
18. createdat: timestamp with time zone, NOT NULL, default NOW()



### 7.2 Tabla telemetryevents

1. Campos y tipos:
2. id: bigint, PK, identity.
3. eventtype: character varying(64), NOT NULL.
4. route: character varying(512), NOT NULL.
5. target: character varying(512), NULL.
6. patientid: character varying(128), NULL.
7. sessionid: character varying(128), NULL.
8. source: character varying(64), NULL.
9. metadatajson: text, NULL.
10. browser: character varying(64), NULL.
11. clientip: character varying(64), NULL.
12. devicetype: character varying(32), NULL.
13. latitude: double precision, NULL.
14. longitude: double precision, NULL.
15. osname: character varying(64), NULL.
16. osversion: character varying(32), NULL.
17. useragent: character varying(1024), NULL.
18. createdat: timestamp with time zone, NOT NULL, default NOW()



### 7.3 Tablas de Identity (autenticación y autorización)

Tablas principales:

1. AspNetUsers: usuarios administradores del panel.
2. AspNetRoles: roles definidos por Identity (base para autorización).
3. AspNetUserRoles: relación N:N entre usuarios y roles.
4. AspNetUserClaims: claims asociados a usuarios.
5. AspNetUserLogins: logins externos por usuario (si se usan proveedores externos).
6. AspNetUserTokens: tokens de autenticación y recuperación por usuario.
7. AspNetRoleClaims: claims asociados a roles.

Relaciones clave:

1. AspNetUsers se relaciona con AspNetRoles mediante AspNetUserRoles.
2. AspNetUsers se relaciona con AspNetUserClaims, AspNetUserLogins y AspNetUserTokens por clave de usuario.
3. AspNetRoles se relaciona con AspNetRoleClaims por clave de rol.



### 7.3 Tipos de eventos



### Los tipos de evento de telemetría no son fijos. La lista puede cambiar en el tiempo según nuevas necesidades funcionales y analíticas del proyecto.

La API de ingesta solo acepta los tipos configurados en `LogIngestion:Telemetry:AllowedEventTypes`; cualquier otro se rechaza. Para agregar un tipo nuevo hay que sumarlo a esa lista (y a los filtros del panel) antes de emitirlo desde Portal MiSalud.

Tipos vigentes:


| eventtype      | Descripción                                         | target (ejemplos)                                                                                                                                                                               | metadata                                                 |
| -------------- | --------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------- |
| `view_page`    | El paciente abrió una página del portal.            | — (se usa `route`)                                                                                                                                                                              | —                                                        |
| `click`        | El paciente pulsó un botón, enlace o ítem de menú.  | `login/submit`, `registro/submit`, `menu/*`, `turnos/confirmar`, `turnos/cancelar`, `telesalud/aceptar-consentimiento`, `programas/responder`, `misdatos/guardar`, `farmacias/buscar-localidad` | Según la acción (`turnoId`, `efector`, `programa`, etc.) |
| `download_pdf` | El paciente descargó un documento en PDF.           | `laboratorio/pdf`, `prestacion/pdf`                                                                                                                                                             | `laboratorioId` o `prestacionId` + `tipoPrestacion`      |
| `view_image`   | El paciente visualizó una imagen de una prestación. | `prestacion/imagen`                                                                                                                                                                             | `prestacionId`, `tipoPrestacion`                         |
| `copy_link`    | El paciente copió el enlace de una prestación.      | `prestacion/enlace`                                                                                                                                                                             | `prestacionId`, `tipoPrestacion`                         |


Los eventos de autenticación, registro, cambio de datos y consentimientos se registran hoy como `click` y se distinguen por su `target` (por ejemplo `login/submit`, `crearcontrasenia/submit`, `misdatos/guardar`, `programas/responder`).

### 7.4 Estrategia de indexación

Índices mínimos requeridos:

1. Por timestamp en logs.
2. Por patientid en logs.
3. Por clientip en logs.
4. Por timestamp en telemetryevents.
5. Por eventtype en telemetryevents.
6. Por patientid en telemetryevents.
7. Por clientip en telemetryevents.
8. Por sessionid y timestamp en telemetryevents (recorrido de sesión).
9. Por sessionid en logs (recorrido de sesión).



## 8 Requerimientos funcionales

1. FR-01: Registrar logs enviados por Portal MiSalud.
2. FR-02: Registrar telemetría enviada por Portal MiSalud.
3. FR-03: Validar API key para todos los endpoints.
4. FR-04: Validar campos obligatorios de cada tipo de payload.
5. FR-05: Si no hay conexión con la API, almacenar logs y telemetría en el navegador y enviarlos automáticamente cuando se restablezca la conexión.
6. FR-06: En el módulo Logs, mostrar listado paginado con filtros avanzados por búsqueda, rango temporal, nivel, categoría, dispositivo, paciente, sesión y mensaje de error, con 30 registros por página.
7. FR-07: En el módulo Logs, mostrar detalle completo del evento.
8. FR-08: En el módulo Logs, mostrar estadísticas de salud del sistema mediante widgets con errores críticos activos y total de errores del día.
9. FR-09: En el módulo Logs, permitir exportación de reportes en CSV, JSON y PDF.
10. FR-10: En el módulo Telemetría, mostrar lista paginada de eventos con filtros por tipo de evento, paciente, rango temporal y búsqueda, con 30 registros por página.
11. FR-11: En el módulo Telemetría, mostrar detalle de evento con contexto cliente y metadata.
12. FR-12: En el módulo Telemetría, mostrar mapa de calor geográfico basado en geolocalización o IP.
13. FR-13: En el módulo Telemetría, permitir exportación de reportes en CSV, JSON y PDF.
14. FR-14: Restringir acceso del panel a usuarios autenticados.
15. FR-15: Gestionar usuarios administradores mediante Identity, incluyendo alta, edición y eliminación.
16. FR-16: Mantener permisos equivalentes para todos los administradores.
17. FR-17: Restringir la eliminación accidental del usuario autenticado actual, si se define esta política de UI.
18. FR-18: Implementar particionamiento por rango de fecha (mensual o trimestral) en las tablas de alto volumen.
19. FR-19: Registrar en el log de auditoría todo intento de acceso al dashboard (exitoso o fallido) como evento del sistema de dashboards, y permitir su visualización en el panel administrativo.
20. FR-20: En el módulo Telemetría, permitir visualizar el recorrido de un usuario durante su sesión como una línea de tiempo cronológica de eventos de telemetría y logs asociados al mismo sessionid, con resumen de la sesión (paciente, inicio, fin, duración, páginas visitadas, acciones y errores), según lo descripto en la sección 5.1.
21. FR-21: En el módulo Telemetría, mostrar un listado paginado de sesiones con filtros por paciente y rango temporal, con acceso directo al recorrido de cada sesión.



## 9 Requerimientos no funcionales

1. NFR-01 Seguridad en tránsito: HTTPS obligatorio.
2. NFR-02 Seguridad de ingesta: API key obligatoria en endpoints de captura.
3. NFR-03 Integridad: rechazo de payloads inválidos con 400.
4. NFR-04 Rendimiento: soporte de paginación y filtros sobre índices existentes.
5. NFR-05 Disponibilidad: arranque con migraciones aplicadas en entorno controlado.
6. NFR-06 Trazabilidad: correlación por patientid y sessionid.
7. NFR-07 Privacidad: manejo controlado de IP, useragent y datos de contexto cliente.
8. NFR-08 Escalabilidad evolutiva: diseño extensible a nuevos tipos de evento.



## 10 Reglas de validación y normalización

1. Logs: level es obligatorio.
2. Telemetría: eventtype y route son obligatorios.
3. metadatajson debe ser JSON válido cuando se recibe.
4. Campos de texto se normalizan y recortan según longitud máxima de base de datos.
5. clientip se toma del payload si viene válido; en caso contrario se resuelve desde contexto HTTP.
6. Los eventos almacenados temporalmente en navegador deben conservar su payload original y metadatos de trazabilidad para envío posterior.



## 11 Seguridad y cumplimiento

1. El panel administrativo requiere autenticación.
2. La API de ingesta utiliza API key compartida.
3. Debe evitarse la exposición de secretos en código o repositorio.
4. Se recomienda rotación periódica de API key.
5. Se recomienda política de contraseñas fuertes para cuentas admin.



## 12 Operación y mantenimiento

1. Monitorear volumen de ingesta por día y tasa de error HTTP.
2. Monitorear crecimiento de tablas logs y telemetryevents.
3. Definir retención inicial de datos (recomendado: 6 a 12 meses).
4. Definir estrategia de respaldo y recuperación.
5. Definir procedimiento de incidentes para caída de API o DB.



## 13 Criterios de aceptación

1. Portal MiSalud puede enviar logs y telemetría con respuesta 204 en casos válidos.
2. Eventos inválidos se rechazan con 400.
3. Solicitudes sin API key válida se rechazan con 401.
4. Los datos quedan persistidos y visibles en panel admin.
5. Se pueden administrar usuarios administradores autenticados.
6. Dado un sessionid, el panel muestra el recorrido completo de la sesión en orden cronológico, incluyendo páginas visitadas, acciones y errores asociados.



## 14 Riesgos conocidos

1. Dependencia temporal de sessionid generado localmente por el portal.
2. Crecimiento acelerado de datos sin política de retención puede afectar desempeño.
3. API key compartida requiere gobernanza para evitar exposición.



## 15 Trazabilidad con implementación actual

La especificación se alinea con:

1. API de ingesta de logs y telemetría en el proyecto administrativo.
2. Entidades de datos logs y telemetryevents en EF Core.
3. Migraciones de PostgreSQL ya creadas para esquema y campos de contexto cliente.
4. Cliente de envío desde Portal MiSalud hacia la API del admin.

