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
        // No TokenSecurity.cs, adicione um parâmetro para especificar a audience
        public string CreateToken(Guid usuarioId, string userRole, string audience = "VibeService")
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtTokenSettings.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, usuarioId.ToString()),
            new Claim(ClaimTypes.Role, userRole)
        }),
                Issuer = "ControlApp",
                Audience = audience,  // Agora usa o parâmetro
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}