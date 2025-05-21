using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Dtos.Request
{
    public class LogoutRequestDto
    {
        public Guid UsuarioId { get; set; }
        public string Token { get; set; } = null!;
    }
}
