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
                            "http://localhost:4200",
                            "http://69.62.104.13:5000",
                            "https://69.62.104.13:5000",
                            "http://localhost:4200",
                            "https://cedae.vibetex.com.br",
                            "http://cedae.vibetex.com.br",
                            "http://69.62.104.13",
                            "https://69.62.104.13"

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