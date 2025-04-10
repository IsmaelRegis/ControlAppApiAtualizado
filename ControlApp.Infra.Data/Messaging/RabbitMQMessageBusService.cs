using System;
using System.Text;
using System.Text.Json;
using ControlApp.Domain.Dtos.Messaging;
using ControlApp.Domain.Interfaces.Messages;
using RabbitMQ.Client;

namespace ControlApp.Infra.Data.Messaging
{
    public class RabbitMQMessageBusService : IMessageBusService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly RabbitMQ.Client.IModel _channel;
        private const string WelcomeQueueName = "welcome-queue";

        public RabbitMQMessageBusService(string hostName = "172.17.0.3", int port = 5672)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = hostName,
                    UserName = "guest",
                    Password = "guest",
                    Port = port,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(30)
                };

                Console.WriteLine($"Conectando ao RabbitMQ em {hostName}:{port}...");
                _connection = factory.CreateConnection();
                Console.WriteLine("Conexão com RabbitMQ estabelecida com sucesso!");

                _channel = _connection.CreateModel();

                // Declarando a fila para garantir que ela existe
                _channel.QueueDeclare(
                    queue: WelcomeQueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao conectar ao RabbitMQ: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Causa: {ex.InnerException.Message}");
                throw;
            }
        }

        public void PublishWelcomeMessage(WelcomeMessage welcomeMessage)
        {
            try
            {
                string messageJson = JsonSerializer.Serialize(welcomeMessage);
                var body = Encoding.UTF8.GetBytes(messageJson);

                Console.WriteLine($"Publicando mensagem de boas-vindas para: {welcomeMessage.Nome} ({welcomeMessage.Email})");

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: WelcomeQueueName,
                    basicProperties: null,
                    body: body);

                Console.WriteLine("Mensagem publicada com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao publicar mensagem: {ex.Message}");
                throw;
            }
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

                Console.WriteLine("Conexão com RabbitMQ fechada com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao fechar conexão com RabbitMQ: {ex.Message}");
            }
        }
    }
}