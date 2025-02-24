using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ControlApp.Infra.Security.Settings;

namespace ControlApp.API.Configurations
{
    public class JwtBearerConfiguration
    {
        public static void Configure(IServiceCollection services, IConfiguration configuration)
        {

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
              .AddJwtBearer(options =>
              {
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuer = true,
                      ValidIssuer = "ControlApp",
                      ValidateAudience = true,
                      ValidAudience = "VibeService",
                      ValidateLifetime = false,

                      ValidateIssuerSigningKey = true,
                      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTokenSettings.Key)),
                      ClockSkew = TimeSpan.Zero
                  };

                  options.Events = new JwtBearerEvents
                  {
                      OnChallenge = context =>
                      {
                          context.HandleResponse();
                          context.Response.StatusCode = 401;
                          context.Response.ContentType = "application/json";
                          return context.Response.WriteAsync("{\"Error\": \"Acesso negado. Token inválido ou não fornecido.\"}");
                      }
                  };
              });
        }
    }
}
