using ControlApp.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1); // Executar a cada hora

    public TokenCleanupService(
        IServiceProvider serviceProvider,
        ILogger<TokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Token Cleanup Service está iniciando.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredTokensAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar tokens expirados");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredTokensAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

        var now = DateTime.UtcNow;
        var expiredTokens = await dbContext.UserTokens
            .Where(t => t.ExpiresAt < now || !t.IsActive)
            .ToListAsync();

        if (expiredTokens.Any())
        {
            _logger.LogInformation($"Removendo {expiredTokens.Count} tokens expirados");
            dbContext.UserTokens.RemoveRange(expiredTokens);
            await dbContext.SaveChangesAsync();
        }
    }
}