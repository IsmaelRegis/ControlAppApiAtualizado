using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ControlApp.API.Configurations;
using ControlApp.Infra.Data.Contexts;
using ControlApp.Infra.Data.Seeders;
using ControlApp.Infra.Data.Identity;
using Microsoft.AspNetCore.Identity;
using ControlApp.API.Middlewares;
/*using ControlApp.Infra.Data.MongoDB.Configurations;
using ControlApp.Infra.Data.Services;*/

var builder = WebApplication.CreateBuilder(args);

#region Configura��o de Servi�os
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Adiciona conversor para enums como strings no JSON
    });

// Configura��es do banco de dados SQL Server
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // Configura o contexto do banco com SQL Server

/*// Configura��es do MongoDB
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbContext>();

// Configurar os mapeamentos do MongoDB
MongoDbConfig.ConfigureMongoDbMappings();*/

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders(); // Configura Identity com usu�rios e roles
#endregion

#region Configura��es Adicionais
builder.Services.AddRouting(options => options.LowercaseUrls = true); // Configura URLs em min�sculas
builder.Services.AddScoped<DataSeeder>(); // Registra o seeder de dados como scoped
builder.Services.AddHostedService<TokenCleanupService>();
#endregion

#region Configura��o do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<TimeSpan>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "duration"
    }); // Mapeia TimeSpan como string no Swagger

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira 'Bearer' [espa�o] e o token JWT.\n\nExemplo: 'Bearer seuTokenAqui'"
    }); // Define autentica��o Bearer no Swagger

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    }); // Adiciona requisito de seguran�a Bearer
});
#endregion

#region Configura��es Personalizadas
CorsConfiguration.AddCorsConfiguration(builder.Services); // Adiciona configura��o de CORS
DependencyInjectionConfiguration.AddDependencyInjection(builder.Services); // Adiciona inje��o de depend�ncias
JwtBearerConfiguration.Configure(builder.Services, builder.Configuration); // Configura autentica��o JWT
#endregion

var app = builder.Build();

#region Configura��o de Arquivos Est�ticos
app.UseStaticFiles();
app.UseFileServer(new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images")), // Define a pasta que ir� armazenar as imagens
    RequestPath = "/images", // Define caminho de requisi��o para imagens
    EnableDirectoryBrowsing = false // Desativa navega��o no diret�rio
});
#endregion

#region Inicializa��o do Banco de Dados
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Migra��o para SQL Server
    var dbContext = services.GetRequiredService<DataContext>();
    dbContext.Database.Migrate(); // Aplica migra��es ao banco de dados

    var dataSeeder = services.GetRequiredService<DataSeeder>();
    await dataSeeder.SeedAsync(); // Executa o seeding de dados iniciais

    /*// Sincroniza��o inicial com MongoDB
    try
    {
        var syncService = services.GetRequiredService<DatabaseSyncService>();
        await syncService.SincronizarTodosAsync();
        Console.WriteLine("Sincroniza��o inicial com MongoDB conclu�da com sucesso");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro durante a sincroniza��o inicial com MongoDB: {ex.Message}");
    }*/
}
#endregion

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Habilita Swagger em ambiente de desenvolvimento
    app.UseSwaggerUI(); // Habilita interface do Swagger
}

#region Pipeline da Aplica��o
app.UseRouting(); // Configura roteamento
app.UseCors("AgendaPolicy"); // Aplica pol�tica de CORS
app.UseAuthentication(); // Habilita autentica��o
app.UseActiveTokenValidation(); // Adiciona middleware de valida��o de token ativo
app.UseAuthorization(); // Habilita autoriza��o
app.MapControllers(); // Mapeia os controladores

#region Configura��o de URLs
// Verifica se a aplica��o est� sendo executada no Docker
string aspNetCoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
bool isRunningInDocker = !string.IsNullOrEmpty(aspNetCoreUrls);

// Se n�o estiver rodando no Docker, configure as URLs manualmente
if (!isRunningInDocker)
{
    if (app.Environment.IsDevelopment())
    {
        app.Urls.Add("http://0.0.0.0:5001"); // Porta para desenvolvimento local
        Console.WriteLine("Aplica��o configurada para executar na porta 5001 em ambiente de desenvolvimento");
    }
    else
    {
        app.Urls.Add("http://0.0.0.0:6000"); // Porta para produ��o local
        Console.WriteLine("Aplica��o configurada para executar na porta 5000 em ambiente de produ��o");
    }
}
else
{
    Console.WriteLine($"Executando no Docker com ASPNETCORE_URLS={aspNetCoreUrls}");
}
#endregion

app.Run();
#endregion