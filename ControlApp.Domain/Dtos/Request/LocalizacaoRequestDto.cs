using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Dtos.Request
{
    public class LocalizacaoRequestDto
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime DataHora { get; set; }
        public double Precisao { get; set; }
        public double DistanciaPercorrida { get; set; }
    }
}
