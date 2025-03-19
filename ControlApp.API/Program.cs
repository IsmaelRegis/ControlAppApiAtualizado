using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ControlApp.API.Configurations;
using ControlApp.Infra.Data.Contexts;
using ControlApp.Infra.Data.Seeders;
using ControlApp.Infra.Data.Identity;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

#region Configuração de Serviços
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Adiciona conversor para enums como strings no JSON
    });

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // Configura o contexto do banco com SQL Server

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders(); // Configura Identity com usuários e roles
#endregion

#region Configurações Adicionais
builder.Services.AddRouting(options => options.LowercaseUrls = true); // Configura URLs em minúsculas
builder.Services.AddScoped<DataSeeder>(); // Registra o seeder de dados como scoped
#endregion

#region Configuração do Swagger
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
        Description = "Insira 'Bearer' [espaço] e o token JWT.\n\nExemplo: 'Bearer seuTokenAqui'"
    }); // Define autenticação Bearer no Swagger

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
    }); // Adiciona requisito de segurança Bearer
});
#endregion

#region Configurações Personalizadas
CorsConfiguration.AddCorsConfiguration(builder.Services); // Adiciona configuração de CORS
DependencyInjectionConfiguration.AddDependencyInjection(builder.Services); // Adiciona injeção de dependências
JwtBearerConfiguration.Configure(builder.Services, builder.Configuration); // Configura autenticação JWT
#endregion

var app = builder.Build();

#region Configuração de Arquivos Estáticos
app.UseStaticFiles();
app.UseFileServer(new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images")), // Define a pasta que irá armazenar as imagens
    RequestPath = "/images", // Define caminho de requisição para imagens
    EnableDirectoryBrowsing = false // Desativa navegação no diretório
});
#endregion

#region Inicialização do Banco de Dados
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<DataContext>();
    dbContext.Database.Migrate(); // Aplica migrações ao banco de dados

    var dataSeeder = services.GetRequiredService<DataSeeder>();
    await dataSeeder.SeedAsync(); // Executa o seeding de dados iniciais
}
#endregion
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Habilita Swagger em ambiente de desenvolvimento
    app.UseSwaggerUI(); // Habilita interface do Swagger

}

#region Pipeline da Aplicação
app.UseRouting(); // Configura roteamento
app.UseCors("AgendaPolicy"); // Aplica política de CORS
app.UseAuthentication(); // Habilita autenticação
app.UseAuthorization(); // Habilita autorização
app.MapControllers(); // Mapeia os controladores
app.Urls.Add("http://0.0.0.0:5000");
app.Run();
#endregion