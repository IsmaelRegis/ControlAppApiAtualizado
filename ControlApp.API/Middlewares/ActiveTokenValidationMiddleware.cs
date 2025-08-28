using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using ControlApp.Domain.Interfaces.Security;
using Microsoft.AspNetCore.Http;

namespace ControlApp.API.Middlewares
{
    public class ActiveTokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ActiveTokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITokenManager tokenManager)
        {
            // Pula verificação para rotas públicas ou de autenticação
            if (!context.Request.Path.StartsWithSegments("/api/auth"))
            {
                // Verifica se o usuário está autenticado
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    // Obtém o token da requisição
                    string authHeader = context.Request.Headers["Authorization"];
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        string token = authHeader.Substring("Bearer ".Length).Trim();
                        // Obtém o ID do usuário das claims
                        // Valida se o token é ativo
                        bool isTokenValid = await tokenManager.ValidateTokenAsync(token);
                        if (!isTokenValid)
                        {
                            context.Response.StatusCode = 440;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync("{\"Error\": \"MultipleLoginConflict\", \"Message\": \"Sua conta foi acessada em outro dispositivo. Por favor, faça login novamente.\"}");
                            return;
                        }


                    }
                }
            }
            // Continua a execução da pipeline
            await _next(context);
        }
    }

    // Extensão para registrar o middleware
    public static class ActiveTokenValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseActiveTokenValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ActiveTokenValidationMiddleware>();
        }
    }
}