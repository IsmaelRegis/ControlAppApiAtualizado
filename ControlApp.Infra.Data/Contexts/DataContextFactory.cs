using System;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ControlApp.Infra.Data.Contexts
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            // Definir o caminho base corretamente
            var basePath = Directory.GetCurrentDirectory();

            // Ajustar para o diretório do projeto principal, se necessário
            if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
            {
                basePath = Path.Combine(basePath, "..", "ControlApp.API"); // Altere para o projeto que contém appsettings.json
            }

            // Construir a configuração
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configurar DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new DataContext(optionsBuilder.Options);
        }
    }
}
