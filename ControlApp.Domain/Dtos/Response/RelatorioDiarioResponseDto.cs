using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Dtos.Response
{
    public class RelatorioDiarioResponseDto
    {
        public Guid UsuarioId { get; set; }
        public string NomeTecnico { get; set; }
        public DateTime Data { get; set; }
        public Guid PontoExpedienteId { get; set; }
        public DateTime? InicioExpediente { get; set; }
        public DateTime? FimExpediente { get; set; }
        public TimeSpan? HorasTrabalhadas { get; set; }
        public string FotoInicioExpediente { get; set; }
        public string FotoFimExpediente { get; set; }
        public List<PausaInfoResponseDto> Pausas { get; set; }
        public TimeSpan? HorasExtras { get; set; }
        public TimeSpan? HorasDevidas { get; set; }
        public string? LatitudeInicioExpediente { get; set; }
        public string? LongitudeInicioExpediente { get; set; }
        public string? LatitudeFimExpediente { get; set; }
        public string? LongitudeFimExpediente { get; set; }
        public string? Observacoes { get; set; }

        public RelatorioDiarioResponseDto()
        {
            Pausas = new List<PausaInfoResponseDto>();
        }
    }
}
