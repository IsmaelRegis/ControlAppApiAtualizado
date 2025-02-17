using ControlApp.Infra.Security.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ControlApp.API.Configurations
{
    public class JwtBearerConfiguration
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Não valida emissor e público-alvo, conforme a configuração original
                    ValidateIssuer = false,
                    ValidateAudience = false,

                    // Valida o tempo de vida do token
                    ValidateLifetime = false,

                    // Chave usada para validar a assinatura do token
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTokenSettings.Key)),

                    // Tolerância ao tempo de validação, permitindo pequenas diferenças de horário
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                // Configuração para personalizar a resposta de falha de autenticação
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse(); // Suprime a resposta padrão do 401
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync(new
                        {
                            Error = "Acesso negado. Token inválido ou não fornecido."
                        }.ToString());
                    }
                };
            });
        }
    }
}

