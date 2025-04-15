using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ControlApp.Infra.Security.Settings;

namespace ControlApp.API.Configurations
{
    public class JwtBearerConfiguration
    {
        #region Configuração de Autenticação JWT
        // Método estático para configurar a autenticação JWT no IServiceCollection
        public static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            /* 
             * Configura a autenticação baseada em JWT (JSON Web Token) 
             * definindo esquemas padrão e parâmetros de validação do token.
             */
            services.AddAuthentication(options =>
            {
                // Define o esquema padrão de autenticação e desafio como JWT Bearer
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                #region Parâmetros de Validação do Token
                // Configura os parâmetros de validação do token JWT
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,              // Valida o emissor do token
                    ValidIssuer = "ControlApp",         // Define o emissor válido como "ControlApp"
                    ValidateAudience = true,            // Valida a audiência do token
                    ValidAudiences = new[] { "VibeService", "CedaeApp" },  // Aceita múltiplas audiences
                    ValidateLifetime = true,            // Altera para true para validar expiração do token
                    ValidateIssuerSigningKey = true,    // Valida a chave de assinatura do emissor
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTokenSettings.Key)), // Chave simétrica para assinatura
                    ClockSkew = TimeSpan.Zero           // Remove tolerância de tempo para expiração
                };
                #endregion

                #region Eventos de Autenticação
                // Configura eventos personalizados para o JWT Bearer
                options.Events = new JwtBearerEvents
                {
                    // Evento disparado quando a autenticação falha (ex.: token inválido)
                    OnChallenge = context =>
                    {
                        context.HandleResponse(); // Impede o comportamento padrão de redirecionamento
                        context.Response.StatusCode = 401; // Define o status HTTP como 401 (Não Autorizado)
                        context.Response.ContentType = "application/json"; // Define o tipo de conteúdo como JSON
                        // Retorna uma mensagem de erro 
                        return context.Response.WriteAsync("{\"Error\": \"Acesso negado. Token inválido ou não fornecido.\"}");
                    }
                };
                #endregion
            });
        }
        #endregion
    }
}