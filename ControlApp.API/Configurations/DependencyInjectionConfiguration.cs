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

            /*#region Repositórios do MongoDB
            // Registro dos repositórios base do MongoDB
            services.AddScoped<BaseRepository<Usuario>>(provider => {
                var context = provider.GetRequiredService<MongoDbContext>();
                return new BaseRepository<Usuario>(context, "usuarios");
            });

            services.AddScoped<BaseRepository<Ponto>>(provider => {
                var context = provider.GetRequiredService<MongoDbContext>();
                return new BaseRepository<Ponto>(context, "pontos");
            });

            services.AddScoped<BaseRepository<Trajeto>>(provider => {
                var context = provider.GetRequiredService<MongoDbContext>();
                return new BaseRepository<Trajeto>(context, "trajetos");
            });

            services.AddScoped<BaseRepository<Localizacao>>(provider => {
                var context = provider.GetRequiredService<MongoDbContext>();
                return new BaseRepository<Localizacao>(context, "localizacoes");
            });

            services.AddScoped<BaseRepository<Empresa>>(provider => {
                var context = provider.GetRequiredService<MongoDbContext>();
                return new BaseRepository<Empresa>(context, "empresas");
            });

            // Serviço de sincronização de dados entre SQL Server e MongoDB
            services.AddScoped<DatabaseSyncService>();

            // Adiciona o serviço de background para sincronização periódica
            services.AddHostedService<DatabaseSyncBackgroundService>();
            #endregion*/

            #region Repositórios
            // Registro de repositórios com suas interfaces correspondentes
            services.AddTransient<ILocalizacaoRepository, LocalizacaoRepository>(); // Repositório de Localização
            services.AddTransient<ITrajetoRepository, TrajetoRepository>();         // Repositório de Trajetos
            services.AddTransient<IUsuarioRepository, UsuarioRepository>();         // Repositório de Usuários
            services.AddTransient<ITecnicoRepository, TecnicoRepository>();         // Repositório de Técnicos
            services.AddTransient<IPontoRepository, PontoRepository>();             // Repositório de Pontos
            services.AddTransient<IEmpresaRepository, EmpresaRepository>();         // Repositório de Empresas
            services.AddTransient<IAuditoriaRepository, AuditoriaRepository>();     // Repositório de Auditorias
            #endregion

            #region Entidades e Interfaces de Usuário
            // Registro de implementações concretas para a interface IUsuario
            services.AddTransient<IUsuario, Tecnico>();         // Técnico como implementação de IUsuario
            services.AddTransient<IUsuario, Administrador>();   // Administrador como implementação de IUsuario
            services.AddTransient<IUsuario, SuperAdministrador>(); // SuperAdministrador como implementação de IUsuario
            #endregion

            #region Serviços de Segurança
            // Registro de serviços relacionados à segurança
            services.AddTransient<ITokenSecurity, TokenSecurity>(); // Serviço de geração e validação de tokens
            services.AddScoped<CryptoSHA256>();                     // Serviço de criptografia SHA256 (escopo único)
            services.AddScoped<CryptoAes>();                        // Serviço de criptografia AES (escopo único)
            services.AddScoped<ITokenSecurity, TokenSecurity>();
            services.AddScoped<ITokenManager, TokenManager>();
            #endregion


            #region Regras de Negócio
            // Registro de serviços da aplicação
            services.AddTransient<IImageService, ImageService>();   // Serviço para manipulação de imagens
            services.AddTransient<IPontoService, PontoService>();   // Serviço para lógica de negócios de Ponto
            services.AddTransient<IUsuarioService, UsuarioService>(); // Serviço para lógica de negócios de Usuário
            services.AddTransient<IAuditoriaService, AuditoriaService>(); // Serviço para lógica de negócios de Auditoria
            #endregion
        }
        #endregion
    }
}