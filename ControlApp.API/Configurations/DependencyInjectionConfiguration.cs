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
        #region Configuração de Injeção de Dependência
        // Método estático para configurar a injeção de dependência no IServiceCollection
        public static void AddDependencyInjection(IServiceCollection services)
        {
            /* 
             * Configura as dependências da aplicação, registrando serviços, 
             * repositórios, validadores e classes de segurança no contêiner de DI.
             */

            #region Validadores
            // Registro de validadores
            services.AddTransient<PontoValidator>(); // Validador para a entidade Ponto
            #endregion

            #region Repositórios
            // Registro de repositórios com suas interfaces correspondentes
            services.AddTransient<ILocalizacaoRepository, LocalizacaoRepository>(); // Repositório de Localização
            services.AddTransient<ITrajetoRepository, TrajetoRepository>();         // Repositório de Trajetos
            services.AddTransient<IUsuarioRepository, UsuarioRepository>();         // Repositório de Usuários
            services.AddTransient<ITecnicoRepository, TecnicoRepository>();         // Repositório de Técnicos
            services.AddTransient<IPontoRepository, PontoRepository>();             // Repositório de Pontos
            services.AddTransient<IEmpresaRepository, EmpresaRepository>();         // Repositório de Empresas
            #endregion

            #region Entidades e Interfaces de Usuário
            // Registro de implementações concretas para a interface IUsuario
            services.AddTransient<IUsuario, Tecnico>();         // Técnico como implementação de IUsuario
            services.AddTransient<IUsuario, Administrador>();   // Administrador como implementação de IUsuario
            #endregion

            #region Serviços de Segurança
            // Registro de serviços relacionados à segurança
            services.AddTransient<ITokenSecurity, TokenSecurity>(); // Serviço de geração e validação de tokens
            services.AddScoped<CryptoSHA256>();                     // Serviço de criptografia SHA256 (escopo único)
            services.AddScoped<CryptoAes>();                        // Serviço de criptografia AES (escopo único)
            #endregion

            #region Regras de Negócio
            // Registro de serviços da aplicação
            services.AddTransient<IImageService, ImageService>();   // Serviço para manipulação de imagens
            services.AddTransient<IPontoService, PontoService>();   // Serviço para lógica de negócios de Ponto
            services.AddTransient<IUsuarioService, UsuarioService>(); // Serviço para lógica de negócios de Usuário
            #endregion
        }
        #endregion
    }
}