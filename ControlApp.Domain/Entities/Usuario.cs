using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Enums;
using ControlApp.Domain.Interfaces.Identity;

namespace ControlApp.Domain.Entities
{
    public abstract class Usuario : IUsuario
    {
        public Guid UsuarioId { get; set; } 
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public UserRole Role { get; set; }
        public string? UserName { get; set; }
        public bool Ativo { get; set; } = true;
        public string? TipoUsuario { get; set; }
        public string? FotoUrl { get; set; }
        public DateTime? DataHoraUltimaAutenticacao { get; set; }
    }

}
