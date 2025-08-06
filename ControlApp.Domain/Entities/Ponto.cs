using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;

public class Ponto
{
    public Guid Id { get; set; }
    public Guid? ExpedienteId { get; set; } 

    public DateTime? InicioExpediente { get; set; }
    public DateTime? FimExpediente { get; set; }
    public DateTime? InicioPausa { get; set; }
    public DateTime? RetornoPausa { get; set; }
    public TimeSpan HorasTrabalhadas { get; set; }
    public TimeSpan HorasExtras { get; set; }
    public TimeSpan HorasDevidas { get; set; }
    public string? LatitudeInicioExpediente { get; set; }
    public string? LongitudeInicioExpediente { get; set; }
    public double? LatitudeInicioPausa { get; set; }
    public double? LongitudeInicioPausa { get; set; }
    public string? LatitudeFimExpediente { get; set; }
    public string? LongitudeFimExpediente { get; set; }
    public double? LatitudeRetornoPausa { get; set; }
    public double? LongitudeRetornoPausa { get; set; }
    public string? Observacoes { get; set; }
    public TipoPonto TipoPonto { get; set; }
    public Guid UsuarioId { get; set; }
    public Tecnico? Tecnico { get; set; }
    public bool Ativo { get; set; } = true;
    public string? FotoInicioExpediente { get; set; }
    public string? FotoFimExpediente { get; set; }
    public string? DistanciaPercorrida { get; set; }
    public string? ObservacaoInicioExpediente { get; set; }
    public string? ObservacaoFimExpediente { get; set; }
    public string? ObservacaoInicioPausa { get; set; }
    public string? ObservacaoFimPausa { get; set; }
}
