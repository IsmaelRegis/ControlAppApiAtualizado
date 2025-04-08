using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Entities
{
    public class Localizacao
    {
        public Guid  LocalizacaoId { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public DateTime DataHora { get; set; } 
        public double Precisao { get; set; }
        public Guid TrajetoId { get; set; }
        public Trajeto? Trajeto { get; set; }
    }
}

