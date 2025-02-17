using System;
using System.Collections.Generic;

namespace ControlApp.Domain.Dtos.Response
{
    public class TecnicoResponseDto : UsuarioResponseDto
    {
        public string? Cpf { get; set; }
        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraSaida { get; set; }
        public TimeSpan? HoraAlmocoInicio { get; set; }
        public TimeSpan? HoraAlmocoFim { get; set; }
        public bool? IsOnline { get; set; }
        public List<ConsultarPontoResponseDto> Pontos { get; set; } = new();
        public List<TrajetoResponseDto> Trajetos { get; set; } = new();
    }
}
