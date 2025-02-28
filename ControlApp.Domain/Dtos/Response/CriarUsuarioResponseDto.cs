using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Enums;

namespace ControlApp.Domain.Dtos.Response
{
    public class CriarUsuarioResponseDto
    {
        public Guid UsuarioId { get; set; }
        public string? Nome { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public string? Cpf { get; set; }
        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraSaida { get; set; }
        public TimeSpan? HoraAlmocoInicio { get; set; }
        public TimeSpan? HoraAlmocoFim { get; set; }
        public string? Role { get; set; }
        public bool Ativo { get; set; }
        public string? FotoUrl { get; set; }  
        public bool IsOnline { get; set; }
        public string? NumeroMatricula { get; set; }
        public Guid? EmpresaId { get; set; }
    }
}
