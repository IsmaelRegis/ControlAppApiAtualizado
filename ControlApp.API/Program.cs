using ControlApp.Infra.Data.Identity;
using ControlApp.Infra.Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using ControlApp.API.Configurations;
using ControlApp.Infra.Data.Seeders;
using ControlApp.Infra.Security.Settings;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Configuração dos Controllers e JsonSerializer para Enums
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configuração do DbContext para conexão com o banco de dados
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do Identity para gerenciamento de usuários
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

// Configuração para aceitar URLs em minúsculas
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Serviço para Seed de dados (criação inicial de dados)
builder.Services.AddScoped<DataSeeder>();

// Configuração do Swagger para documentação da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<TimeSpan>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "duration" });

    // Definição de segurança JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira 'Bearer' [espaço] e o token JWT.\n\nExemplo: 'Bearer seuTokenAqui'"
    });

    // Requisito de segurança global
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
            new string[] { }
        }
    });
});

// Configuração de CORS
CorsConfiguration.AddCorsConfiguration(builder.Services);

// Configuração de injeção de dependências
DependencyInjectionConfiguration.AddDependencyInjection(builder.Services);
JwtBearerConfiguration.Configure(builder.Services);



var app = builder.Build();

app.UseStaticFiles(); 
app.UseFileServer(new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images")),
    RequestPath = "/images",
    EnableDirectoryBrowsing = false
});



// Executando migrações e seed de dados ao iniciar a aplicação
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<DataContext>();

    // Aplicando as migrações no banco de dados
    dbContext.Database.Migrate();

    // Realizando o seed de dados (criação de roles, usuários, etc.)
    var dataSeeder = services.GetRequiredService<DataSeeder>();
    await dataSeeder.SeedAsync();
}

// Configuração de Swagger apenas em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configuração do pipeline de middlewares
app.UseRouting();

// CORS deve ser aplicado antes de autenticação
app.UseCors("AgendaPolicy");

// Adicionando autenticação e autorização
app.UseAuthentication(); // Realiza a autenticação com base no JWT
app.UseAuthorization();  // Utiliza autorizações padrão

// Mapeando os controllers para as rotas
app.MapControllers();

// Iniciando a aplicação
app.Run();
