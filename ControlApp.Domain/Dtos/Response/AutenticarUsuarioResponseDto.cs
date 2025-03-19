using System;

namespace ControlApp.Domain.Dtos.Response
{
    public class AutenticarUsuarioResponseDto
    {
        public Guid UsuarioId { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Token { get; set; }
        public string? Cpf { get; set; }
        public string? FotoUrl { get; set; }
        public string? UserName { get; set; }
        public bool IsOnline { get; set; }
        public string? Mensagem { get; set; }
        public DateTime DataHoraAutenticacao { get; set; }
    }
}
