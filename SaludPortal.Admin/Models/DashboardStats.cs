namespace SaludPortal.Admin.Models;

public record DashboardStats(
    int EventosHoy,
    Dictionary<string, int> EventosPorTipo,
    int PacientesActivasHoy,
    int ErroresHoy);
