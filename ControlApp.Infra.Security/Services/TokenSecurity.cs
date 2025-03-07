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
            var tokenHandler = new JwtSecurityTokenHandler(); // Cria o manipulador de tokens JWT
            var key = Encoding.ASCII.GetBytes(JwtTokenSettings.Key); // Pega a chave secreta do settings

            // Configura o token com as claims (ID e role), emissor, audiência e assinatura
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, usuarioId.ToString()),
                    new Claim(ClaimTypes.Role, userRole)
                }),
                Issuer = "ControlApp",
                Audience = "VibeService",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor); // Gera o token
            return tokenHandler.WriteToken(token); // Converte pra string e retorna
        }
    }
}