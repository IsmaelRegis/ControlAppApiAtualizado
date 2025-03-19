namespace ControlApp.API.Configurations
{
    public class CorsConfiguration
    {
        #region Configuração de CORS
        // Método estático para adicionar a configuração de CORS ao IServiceCollection
        public static void AddCorsConfiguration(IServiceCollection services)
        {
            /* 
             * Configura o CORS (Cross-Origin Resource Sharing) para permitir 
             * que a API seja acessada por origens específicas, métodos e cabeçalhos.
             */
            services.AddCors(options =>
            {
                // Define a política de CORS chamada "AgendaPolicy"
                options.AddPolicy("AgendaPolicy", builder =>
                {
                    builder.WithOrigins(
                            "http://212.85.1.124:5030",
                            "https://212.85.1.124:5030",
                            "http://localhost:4200",
                            "https://api.gilvandev.com",
                            "https://gilvandev.com",
                            "https://212.85.1.124",
                            "http://212.85.1.124"
                        )
                        .AllowAnyMethod()   // Permite todos os métodos HTTP (GET, POST, PUT, etc.)
                        .AllowAnyHeader();  // Permite todos os cabeçalhos nas requisições
                });
            });
        }
        #endregion

        #region Aplicação de CORS
        // Método estático para aplicar a política de CORS no pipeline da aplicação
        public static void UseCorsConfiguration(IApplicationBuilder app)
        {
            /* 
             * Habilita o uso da política "AgendaPolicy" definida anteriormente 
             * no pipeline de middleware da aplicação.
             */
            app.UseCors("AgendaPolicy");
        }
        #endregion
    }
}