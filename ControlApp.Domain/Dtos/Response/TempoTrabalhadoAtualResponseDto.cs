using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Dtos.Response
{
    public class TempoTrabalhadoAtualResponseDto
    {
        public string Status { get; set; } 
        public TimeSpan TempoTrabalhadoLiquido { get; set; }
        public Guid? ExpedienteId { get; set; }
        public Guid? PausaAtualId { get; set; }
    }
}
