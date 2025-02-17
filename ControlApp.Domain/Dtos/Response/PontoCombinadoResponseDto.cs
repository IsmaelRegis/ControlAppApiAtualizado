using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Enums;

namespace ControlApp.Domain.Dtos.Response
{
    public class PontoCombinadoResponseDto
    {
        public Guid Id { get; set; }
        public TipoPonto TipoPonto { get; set; }
        public Guid UsuarioId { get; set; }
        public string? Nome { get; set; }
        public DateTime? InicioExpediente { get; set; }
        public DateTime? FimExpediente { get; set; }
        public DateTime? InicioPausa { get; set; }
        public DateTime? RetornoPausa { get; set; }
        public TimeSpan? HorasTrabalhadas { get; set; }
        public TimeSpan? HorasExtras { get; set; }
        public TimeSpan? HorasDevidas { get; set; }
        public double? LatitudeInicioExpediente { get; set; }
        public double? LongitudeInicioExpediente { get; set; }
        public double? LatitudeFimExpediente { get; set; }
        public double? LongitudeFimExpediente { get; set; }
        public double? LatitudeInicioPausa { get; set; }
        public double? LongitudeInicioPausa { get; set; }
        public double? LatitudeRetornoPausa { get; set; }
        public double? LongitudeRetornoPausa { get; set; }
        public string? FotoInicioExpediente { get; set; }
        public string? FotoFimExpediente { get; set; }
        public string? Observacoes { get; set; }
    }

}
