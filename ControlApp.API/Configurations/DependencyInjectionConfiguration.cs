using ControlApp.Domain.Entities;
using ControlApp.Domain.Interfaces.Identity;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Domain.Interfaces.Security;
using ControlApp.Domain.Interfaces.Services;
using ControlApp.Domain.Services;
using ControlApp.Domain.Validations;
using ControlApp.Infra.Data.Repositories;
using ControlApp.Infra.Security.Services;

namespace ControlApp.API.Configurations
{
    public class DependencyInjectionConfiguration
    {
        public static void AddDependencyInjection(IServiceCollection services)
        {
            services.AddTransient<PontoValidator>();
            services.AddTransient<ILocalizacaoRepository, LocalizacaoRepository>();
            services.AddTransient<ITrajetoRepository, TrajetoRepository>();
            services.AddTransient<IUsuarioRepository, UsuarioRepository>();
            services.AddTransient<ITecnicoRepository, TecnicoRepository>();
            services.AddTransient<IPontoRepository, PontoRepository>();
            services.AddTransient<IUsuario, Tecnico>();
            services.AddTransient<IUsuario, Administrador>();
            services.AddTransient<ITokenSecurity, TokenSecurity>();
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<IPontoService, PontoService>();
            services.AddTransient<IUsuarioService, UsuarioService>();
            services.AddScoped<CryptoSHA256>();

        }
    }
}
