
using ControlApp.Domain.Entities;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Domain.Interfaces.Security;
using ControlApp.Domain.Interfaces.Services;

public class DeslogarUsuariosScheduler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeslogarUsuariosScheduler> _logger;
    private DateTime _ultimaExecucao = DateTime.MinValue;

    public DeslogarUsuariosScheduler(IServiceProvider serviceProvider, ILogger<DeslogarUsuariosScheduler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🕒 DeslogarUsuariosScheduler iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var agora = DateTime.Now;

            // Executa se passou de 00:00 e ainda não executou nesse dia
            if (agora.Hour == 0 && agora.Minute >= 1 && _ultimaExecucao.Date != agora.Date)
            {
                _logger.LogWarning($"🚨 Iniciando invalidação de tokens em: {agora}");

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var usuarioRepository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
                    var tokenManager = scope.ServiceProvider.GetRequiredService<ITokenManager>();

                    var usuarios = await usuarioRepository.GetAllAsync();

                    foreach (var usuario in usuarios)
                    {
                        if (usuario is Tecnico tecnico)
                        {
                            await tokenManager.InvalidateTokensForUserAsync(tecnico.UsuarioId);
                        }
                    }

                    _ultimaExecucao = agora;
                    _logger.LogWarning("✅ Tokens de técnicos invalidados com sucesso.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao invalidar tokens.");
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Verifica a cada 30s
        }
    }

}
