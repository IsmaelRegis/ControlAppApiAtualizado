using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Enums;

namespace ControlApp.Domain.Dtos.Response
{
    public class ConsultarTecnicoResponseDto
    {
        public Guid UsuarioId { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public UserRole Role { get; set; }
        public bool Ativo { get; set; }
        public string? Cpf { get; set; }
        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraSaida { get; set; }
        public TimeSpan? HoraAlmocoInicio { get; set; }
        public TimeSpan? HoraAlmocoFim { get; set; }
        public string? FotoUrl { get; set; }  
        public bool IsOnline { get; set; }  
        public List<ConsultarPontoResponseDto> Pontos { get; set; } = new List<ConsultarPontoResponseDto>();
        public List<TrajetoResponseDto> Trajetos { get; set; } = new List<TrajetoResponseDto>();
    }
}


