/*using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ControlApp.Infra.Data.Services
{
    public class DatabaseSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseSyncBackgroundService> _logger;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(15); // Configurável

        public DatabaseSyncBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<DatabaseSyncBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de sincronização MongoDB iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var syncService = scope.ServiceProvider.GetRequiredService<DatabaseSyncService>();
                        await syncService.SincronizarTodosAsync();
                        _logger.LogInformation("Sincronização periódica concluída com sucesso");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro na sincronização programada");
                }

                await Task.Delay(_syncInterval, stoppingToken);
            }
        }
    }
}*/