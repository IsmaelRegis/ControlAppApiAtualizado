using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Dtos.Request
{
    public class AutenticarUsuarioRequestDto
    {
        public string? Senha { get; set; }
        public string? Cpf { get; set; }
        public string? UserName { get; set; }
    }
}
