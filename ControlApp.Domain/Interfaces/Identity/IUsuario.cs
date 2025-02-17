using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Enums;

namespace ControlApp.Domain.Interfaces.Identity
{
    public interface IUsuario
    {
        Guid UsuarioId { get; set; }
        string? Nome { get; set; }
        string? Email { get; set; }
        UserRole Role { get; set; }
        string? UserName { get; set; }
    }
}
