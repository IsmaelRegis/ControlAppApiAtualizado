using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ControlApp.Infra.Data.Messaging
{
    public class WelcomeMessageBackgroundService : BackgroundService
    {
        private readonly WelcomeMessageConsumer _consumer;

        public WelcomeMessageBackgroundService()
        {
            try
            {
                Console.WriteLine("Iniciando serviço de background para processamento de mensagens de boas-vindas...");
                // Use o IP específico do container RabbitMQ
                _consumer = new WelcomeMessageConsumer(
                    hostName: "172.17.0.3",
                    userName: "guest",
                    password: "guest",
                    port: 5672);
                Console.WriteLine("Serviço de background inicializado com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao inicializar serviço de background: {ex.Message}");
                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Console.WriteLine("Executando serviço de background...");
                // Inicia o consumo de mensagens
                _consumer.StartConsuming();
                Console.WriteLine("Serviço de background em execução");

                // Mantém o serviço em execução até a aplicação ser encerrada
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao executar serviço de background: {ex.Message}");
                throw;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Parando serviço de background...");
                _consumer.StopConsuming();
                _consumer.Dispose();
                Console.WriteLine("Serviço de background parado com sucesso");
                return base.StopAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao parar serviço de background: {ex.Message}");
                return base.StopAsync(cancellationToken);
            }
        }
    }
}