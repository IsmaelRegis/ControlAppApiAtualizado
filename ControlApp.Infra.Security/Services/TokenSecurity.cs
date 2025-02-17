using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ControlApp.Domain.Interfaces.Security;
using ControlApp.Infra.Security.Settings;
using Microsoft.IdentityModel.Tokens;

namespace ControlApp.Infra.Security.Services
{
    public class TokenSecurity : ITokenSecurity
    {
        public string CreateToken(Guid usuarioId, string userRole)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtTokenSettings.Key);

            // Criação do token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Gravando a identificação do usuário e a role no token
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, usuarioId.ToString()),
                    new Claim(ClaimTypes.Role, userRole) // Adiciona o UserRole ao token
                }),

                // Assinatura antifalsificação do token
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            // Retorna o token gerado
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
