using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Enums;

namespace ControlApp.Domain.Entities
{
    public class Tecnico : Usuario
    {
        public string? Cpf { get; set; }
        public TimeSpan HoraEntrada { get; set; }
        public TimeSpan HoraSaida { get; set; }
        public TimeSpan HoraAlmocoInicio { get; set; }
        public TimeSpan HoraAlmocoFim { get; set; }
        public bool IsOnline { get; set; } = false;
        public DateTime? DataEHoraLocalizacao { get; set; }
        public string? LatitudeAtual { get; set; }
        public string? LongitutdeAtual { get; set; }
        public ICollection<Ponto> Pontos { get; set; } = new List<Ponto>();
        public ICollection<Trajeto> Trajetos { get; set; } = new List<Trajeto>();
        public string? NumeroMatricula { get; set; } 
        public Guid? EmpresaId { get; set; }
        public string? NomeDaEmpresa { get; set; }
        public Empresa? Empresa { get; set; }
    }

}
