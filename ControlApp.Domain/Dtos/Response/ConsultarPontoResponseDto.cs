using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Enums;

namespace ControlApp.Domain.Dtos.Response
{
    public class ConsultarPontoResponseDto
    {

        public Guid Id { get; set; }
        public DateTime? InicioExpediente { get; set; }
        public DateTime? FimExpediente { get; set; }
        public DateTime? InicioPausa { get; set; }
        public DateTime? RetornoPausa { get; set; }
        public TimeSpan HorasTrabalhadas { get; set; }
        public TimeSpan HorasExtras { get; set; }
        public TimeSpan HorasDevidas { get; set; }
        public string? LatitudeInicioExpediente { get; set; }
        public string? LongitudeInicioExpediente { get; set; }
        public DateTime? DataHoraInicioExpediente { get; set; }
        public string? LatitudeFimExpediente { get; set; }
        public string? LongitudeFimExpediente { get; set; }
        public DateTime? DataHoraFimExpediente { get; set; }
        public string? LatitudeInicioPausa { get; set; }
        public string? LongitudeInicioPausa { get; set; }
        public DateTime? DataHoraInicioPausa { get; set; }
        public string? LatitudeRetornoPausa { get; set; }
        public string? LongitudeRetornoPausa { get; set; }
        public DateTime? DataHoraRetornoPausa { get; set; }
        public Guid UsuarioId { get; set; }
        public string? NomeTecnico { get; set; }
        public string? Observacoes { get; set; }
        public string? FotoInicioExpediente { get; set; }
        public string? FotoFimExpediente { get; set; }
        public TipoPonto TipoPonto { get; set; }
        public string? DistanciaPercorrida { get; set; }
        public string? ObservacaoInicioExpediente { get; set; }
        public string? ObservacaoFimExpediente { get; set; }
        public string? ObservacaoInicioPausa { get; set; }
        public string? ObservacaoFimPausa { get; set; }
    }
}
