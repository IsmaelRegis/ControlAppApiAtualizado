using System.IdentityModel.Tokens.Jwt;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Interfaces.Security;
using ControlApp.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ControlApp.Infra.Security.Services
{
    public class TokenManager : ITokenManager
    {
        private readonly DataContext _context;
        private readonly ITokenSecurity _tokenSecurity;

        public TokenManager(DataContext context, ITokenSecurity tokenSecurity)
        {
            _context = context;
            _tokenSecurity = tokenSecurity;
        }

        public async Task<string> GenerateTokenAsync(Guid userId, string userRole, string deviceInfo = null, string audience = "VibeService")
        {
            // Desativa todos os tokens existentes para este usuário
            await InvalidateTokensForUserAsync(userId);

            // Gera um novo token
            string token = _tokenSecurity.CreateToken(userId, userRole, audience);

            // Extrai o ID do token (jti) para armazenar
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var tokenId = jwtToken.Claims.First(c => c.Type == "jti").Value;

            // Salva o novo token no banco de dados
            var userToken = new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = tokenId,  // Armazena apenas o ID do token, não o token completo
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                IsActive = true,
                DeviceInfo = deviceInfo
            };

            await _context.UserTokens.AddAsync(userToken);
            await _context.SaveChangesAsync();

            return token;
        }
        public async Task<bool> ValidateTokenAsync(string token, Guid userId)
        {
            try
            {
                // Extrai o ID do token (jti)
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var tokenId = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;

                if (string.IsNullOrEmpty(tokenId))
                {
                    Console.WriteLine($"Token sem jti claim para usuário {userId}");
                    return false;
                }

                // Verifica se este token ID está ativo no banco
                var userToken = await _context.UserTokens
                    .FirstOrDefaultAsync(t =>
                        t.UserId == userId &&
                        t.Token == tokenId &&
                        t.IsActive);

                return userToken != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao validar token: {ex.Message}");
                return false;
            }
        }
        public async Task InvalidateTokensForUserAsync(Guid userId, string currentToken = null)
        {
            var userTokens = await _context.UserTokens
                .Where(t => t.UserId == userId && t.IsActive)
                .ToListAsync();

            foreach (var token in userTokens)
            {
                // Se currentToken for fornecido, apenas desativa tokens diferentes deste
                if (currentToken == null || token.Token != currentToken)
                {
                    token.IsActive = false;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}