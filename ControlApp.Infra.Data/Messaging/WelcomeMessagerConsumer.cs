using ControlApp.Domain.Dtos.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ControlApp.Infra.Data.Messaging
{
    public class WelcomeMessageConsumer : IDisposable
    {
        private readonly IConnection _connection;
        private readonly RabbitMQ.Client.IModel _channel;
        private readonly EmailService _emailService;
        private readonly string _queueName = "welcome-queue";
        private bool _isConsuming = false;

        public WelcomeMessageConsumer(string hostName = "172.17.0.3",
                                     string userName = "guest",
                                     string password = "guest",
                                     int port = 5672)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = hostName,
                    UserName = userName,
                    Password = password,
                    Port = port,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(30)
                };

                Console.WriteLine($"Consumidor: Conectando ao RabbitMQ em {hostName}:{port}...");
                _connection = factory.CreateConnection();
                Console.WriteLine("Consumidor: Conexão com RabbitMQ estabelecida com sucesso!");

                _channel = _connection.CreateModel();
                _channel.QueueDeclare(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _emailService = new EmailService();
                Console.WriteLine("Consumidor: Serviço de e-mail inicializado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Consumidor: Erro ao conectar ao RabbitMQ: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Consumidor: Causa: {ex.InnerException.Message}");
                throw;
            }
        }

        public void StartConsuming()
        {
            if (_isConsuming)
                return;

            try
            {
                _isConsuming = true;
                Console.WriteLine("Consumidor: Iniciando consumo de mensagens...");

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("Consumidor: Mensagem recebida");

                    try
                    {
                        var welcomeMessage = JsonSerializer.Deserialize<WelcomeMessage>(message);

                        if (welcomeMessage != null && !string.IsNullOrEmpty(welcomeMessage.Email))
                        {
                            Console.WriteLine($"Consumidor: Processando mensagem para: {welcomeMessage.Nome} ({welcomeMessage.Email})");

                            // Enviar o e-mail de boas-vindas
                            await _emailService.SendWelcomeEmailAsync(welcomeMessage.Email, welcomeMessage.Nome ?? "Usuário");
                            Console.WriteLine("Consumidor: E-mail enviado com sucesso");

                            // Confirmar o processamento da mensagem
                            _channel.BasicAck(ea.DeliveryTag, false);
                        }
                        else
                        {
                            Console.WriteLine("Consumidor: Mensagem inválida ou sem e-mail");
                            _channel.BasicNack(ea.DeliveryTag, false, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Consumidor: Erro ao processar mensagem: {ex.Message}");
                        // Em caso de erro, rejeita a mensagem e a devolve para a fila
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };

                _channel.BasicConsume(
                    queue: _queueName,
                    autoAck: false, // Desativa o reconhecimento automático para garantir processamento
                    consumer: consumer);

                Console.WriteLine("Consumidor: Consumo de mensagens iniciado com sucesso");
            }
            catch (Exception ex)
            {
                _isConsuming = false;
                Console.WriteLine($"Consumidor: Erro ao iniciar consumo: {ex.Message}");
                throw;
            }
        }

        public void StopConsuming()
        {
            _isConsuming = false;
            Console.WriteLine("Consumidor: Consumo de mensagens interrompido");
        }

        public void Dispose()
        {
            try
            {
                if (_channel != null && _channel.IsOpen)
                    _channel.Close();
                if (_connection != null && _connection.IsOpen)
                    _connection.Close();

                _channel?.Dispose();
                _connection?.Dispose();

                Console.WriteLine("Consumidor: Recursos liberados com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Consumidor: Erro ao liberar recursos: {ex.Message}");
            }
        }
    }
}