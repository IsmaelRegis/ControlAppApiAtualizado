// ControlApp.Domain.Interfaces.Security/ITokenManager.cs
using System;
using System.Threading.Tasks;

namespace ControlApp.Domain.Interfaces.Security
{
    public interface ITokenManager
    {
        Task<string> GenerateTokenAsync(Guid userId, string userRole, string deviceInfo = null, string audience = "VibeService"); // VibeService ou CedaeApp
        Task<bool> ValidateTokenAsync(string token, Guid userId);
        Task InvalidateTokensForUserAsync(Guid userId, string currentToken = null);
    }
}