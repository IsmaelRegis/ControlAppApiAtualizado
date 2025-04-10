using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Dtos.Messaging
{
    public class WelcomeMessage
    {

        public Guid UsuarioId { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public string Mensagem { get; set; } = "Seu perfil no Vibetex foi cadastrado com sucesso!";
    }
}
