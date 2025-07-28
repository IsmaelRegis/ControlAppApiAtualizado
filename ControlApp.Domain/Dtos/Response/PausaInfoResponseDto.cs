using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Dtos.Response
{
    public class PausaInfoResponseDto
    {
        public Guid PontoId { get; set; }
        public DateTime? InicioPausa { get; set; }
        public DateTime? RetornoPausa { get; set; }
        public TimeSpan Duracao { get; set; }
    }
}
