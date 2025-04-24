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
        public string CreateToken(Guid usuarioId, string userRole, string audience = "VibeService")
        {
            var tokenId = Guid.NewGuid().ToString();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtTokenSettings.Key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, usuarioId.ToString()),
            new Claim(ClaimTypes.Role, userRole),
            new Claim("jti", tokenId)  // Adiciona ID único ao token
        }),
                Expires = DateTime.UtcNow.AddHours(24), // Adiciona expiração de 24 horas
                Issuer = "ControlApp",
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}