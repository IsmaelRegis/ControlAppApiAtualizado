using System;
using ControlApp.Domain.Enums;
using ControlApp.Domain.Interfaces.Identity;
using Microsoft.AspNetCore.Identity;

namespace ControlApp.Infra.Data.Identity
{
    public class ApplicationUser : IdentityUser, IUsuario
    {
        // Implementação da propriedade Nome
        public string? Nome { get; set; }

        // Implementação da propriedade Role
        public UserRole Role { get; set; }

        // Implementação da propriedade UsuarioId, que é o Guid necessário na interface IUsuario
        public Guid UsuarioId { get; set; }
    }
}
