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

// Configura��o dos Controllers e JsonSerializer para Enums
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configura��o do DbContext para conex�o com o banco de dados
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configura��o do Identity para gerenciamento de usu�rios
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

// Configura��o para aceitar URLs em min�sculas
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Servi�o para Seed de dados (cria��o inicial de dados)
builder.Services.AddScoped<DataSeeder>();

// Configura��o do Swagger para documenta��o da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<TimeSpan>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "duration" });

    // Defini��o de seguran�a JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira 'Bearer' [espa�o] e o token JWT.\n\nExemplo: 'Bearer seuTokenAqui'"
    });

    // Requisito de seguran�a global
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

// Configura��o de CORS
CorsConfiguration.AddCorsConfiguration(builder.Services);

// Configura��o de inje��o de depend�ncias
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



// Executando migra��es e seed de dados ao iniciar a aplica��o
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<DataContext>();

    // Aplicando as migra��es no banco de dados
    dbContext.Database.Migrate();

    // Realizando o seed de dados (cria��o de roles, usu�rios, etc.)
    var dataSeeder = services.GetRequiredService<DataSeeder>();
    await dataSeeder.SeedAsync();
}

// Configura��o de Swagger apenas em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configura��o do pipeline de middlewares
app.UseRouting();

// CORS deve ser aplicado antes de autentica��o
app.UseCors("AgendaPolicy");

// Adicionando autentica��o e autoriza��o
app.UseAuthentication(); // Realiza a autentica��o com base no JWT
app.UseAuthorization();  // Utiliza autoriza��es padr�o

// Mapeando os controllers para as rotas
app.MapControllers();

// Iniciando a aplica��o
app.Run();
