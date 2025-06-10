using ControlApp.Domain.Interfaces.Services;

public class TokenExpirationService : BackgroundService
{
    private readonly ILogger<TokenExpirationService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public TokenExpirationService(ILogger<TokenExpirationService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço de Expiração de Token está iniciando.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var proximaExecucao = DateTime.Today.AddDays(1).AddMinutes(1); // 00:01 do próximo dia
            var delay = proximaExecucao - DateTime.Now;

            if (delay > TimeSpan.Zero)
            {
                _logger.LogInformation("Próxima execução em: {Delay}", delay);
                await Task.Delay(delay, stoppingToken);
            }

            _logger.LogInformation("Iniciando a rotina de expiração de tokens diários.");

            using (var scope = _scopeFactory.CreateScope())
            {
                var usuarioService = scope.ServiceProvider.GetRequiredService<IUsuarioService>();

                try
                {
                    await usuarioService.ExpireDailyTokensAsync();
                    _logger.LogInformation("Rotina de expiração de tokens concluída com sucesso.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocorreu um erro ao executar a expiração de tokens diários.");
                }
            }
        }
    }
}