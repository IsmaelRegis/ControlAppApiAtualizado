using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace ControlApp.Domain.Dtos.Request
{
    public class CriarUsuarioRequestDto
    {
        public string? Nome { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public string? Cpf { get; set; }
        public UserRole Role { get; set; }
        public TimeSpan HoraEntrada { get; set; }
        public TimeSpan HoraSaida { get; set; }
        public TimeSpan HoraAlmocoInicio { get; set; }
        public TimeSpan HoraAlmocoFim { get; set; }
        public string? FotoUrl { get; set; }
        public IFormFile? FotoFile { get; set; }
    }

}
