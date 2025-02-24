namespace ControlApp.API.Configurations
{
    public class CorsConfiguration
    {
        public static void AddCorsConfiguration(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AgendaPolicy", builder =>
                {
                    builder.WithOrigins(
                                "http://localhost:4200",
                                 "https://localhost:5030" 
                            )
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        public static void UseCorsConfiguration(IApplicationBuilder app)
        {
            app.UseCors("AgendaPolicy");
        }
    }
}
