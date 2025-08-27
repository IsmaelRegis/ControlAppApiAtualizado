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
                Token = tokenId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                IsActive = true,
                DeviceInfo = deviceInfo
            };

            await _context.UserTokens.AddAsync(userToken);
            await _context.SaveChangesAsync();

            return token;
        }
        public async Task<bool> ValidateTokenAsync(string jti, Guid userId)
        {
            if (string.IsNullOrEmpty(jti))
                return false;

            var userToken = await _context.UserTokens
                .FirstOrDefaultAsync(t =>
                    t.UserId == userId &&
                    t.Token == jti &&
                    t.IsActive);

            return userToken != null;
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

        // Dentro da classe TokenManager

        public async Task<List<Guid>> InvalidateActiveTokensBeforeDateAsync(DateTime cutOffDate)
        {
            // Busca todos os tokens que estão ativos e foram criados antes da data de corte (meia-noite).
            var tokensToExpire = await _context.UserTokens
                .Where(t => t.IsActive && t.CreatedAt < cutOffDate)
                .ToListAsync();

            if (!tokensToExpire.Any())
            {
                return new List<Guid>(); // Retorna lista vazia se não há o que fazer.
            }

            // Pega os Ids dos usuários que serão afetados para retornar à camada de serviço.
            var affectedUserIds = tokensToExpire.Select(t => t.UserId).Distinct().ToList();

            // Invalida cada token
            foreach (var token in tokensToExpire)
            {
                token.IsActive = false;
            }

            // Salva todas as alterações no banco de uma só vez.
            await _context.SaveChangesAsync();

            return affectedUserIds;
        }
    }
}