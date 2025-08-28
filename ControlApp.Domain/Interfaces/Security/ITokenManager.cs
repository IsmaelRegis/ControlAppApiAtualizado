// ControlApp.Domain.Interfaces.Security/ITokenManager.cs
using System;
using System.Threading.Tasks;

namespace ControlApp.Domain.Interfaces.Security
{
    public interface ITokenManager
    {
        Task<string> GenerateTokenAsync(Guid userId, string userRole, string deviceInfo = null, string audience = "VibeService");
        Task<bool> ValidateTokenAsync(string jwtTokenString); // 👈 agora só precisa do token
        Task InvalidateTokensForUserAsync(Guid userId, string currentToken = null);
        Task<List<Guid>> InvalidateActiveTokensBeforeDateAsync(DateTime cutOffDate);
    }


}