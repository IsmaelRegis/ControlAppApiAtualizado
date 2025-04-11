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

            // Salva o novo token no banco de dados
            var userToken = new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                DeviceInfo = deviceInfo
            };

            await _context.UserTokens.AddAsync(userToken);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<bool> ValidateTokenAsync(string token, Guid userId)
        {
            var userToken = await _context.UserTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == token && t.IsActive);

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
    }
}