using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Dtos.Response
{
    public class LocalizacaoResponseDto
    {
        public Guid LocalizacaoId { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public DateTime DataHora { get; set; }
        public double Precisao { get; set; }
    }
}
