using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace ControlApp.Infra.Data.Messaging
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _senderEmail;
        private readonly string _senderName;

        public EmailService()
        {
            // Configuração do SendGrid
            _smtpServer = "smtp.sendgrid.net";
            _smtpPort = 587;
            _smtpUsername = "apikey"; // Sempre use "apikey" para o SendGrid
            _smtpPassword = "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"; // Cole sua chave API do SendGrid aqui
            _senderEmail = "ismael.l.regis@gmail.com"; // Seu email verificado
            _senderName = "Vibetex";

            Console.WriteLine("Serviço de e-mail real inicializado com SendGrid");
        }

        public async Task SendWelcomeEmailAsync(string recipientEmail, string recipientName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_senderName, _senderEmail));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = "Bem-vindo ao Vibetex!";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                        <body>
                            <h1>Olá, {recipientName}!</h1>
                            <p>Seu perfil no Vibetex foi cadastrado com sucesso!</p>
                            <p>Seja bem-vindo à nossa plataforma. Estamos muito felizes em ter você conosco!</p>
                            <p>Atenciosamente,<br>Equipe Vibetex</p>
                        </body>
                    </html>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                Console.WriteLine($"Tentando enviar e-mail para {recipientEmail}...");

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                Console.WriteLine($"E-mail enviado com sucesso para {recipientEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Causa: {ex.InnerException.Message}");

                // Decidimos não lançar a exceção para não interromper o fluxo
                // Se quiser que falhas de e-mail retornem a mensagem à fila, descomente:
                // throw;
            }
        }
    }
}