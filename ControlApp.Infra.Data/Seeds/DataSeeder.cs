using System;
using System.Linq;
using System.Threading.Tasks;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;
using ControlApp.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ControlApp.Infra.Data.Seeders
{
    public class DataSeeder
    {
        private readonly DataContext _context;
        private readonly CryptoSHA256 _cryptoSHA256;

        public DataSeeder(DataContext context, CryptoSHA256 cryptoSHA256)
        {
            _context = context;           // Recebe o contexto do banco
            _cryptoSHA256 = cryptoSHA256; // Recebe o serviço de hash pra senha
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();

            // Criação do Administrador
            if (!_context.Usuarios.Any(u => u.Role == UserRole.Administrador))
            {
                var admin = new Administrador
                {
                    UsuarioId = Guid.NewGuid(),
                    Nome = "Admin",
                    UserName = "admin",
                    Email = "admin@controlapp.com",
                    Senha = _cryptoSHA256.HashPassword("@Admin123"),
                    Role = UserRole.Administrador,
                    Ativo = true,
                    TipoUsuario = "Administrador"
                };

                _context.Usuarios.Add(admin);
            }

            // Criação do SuperAdministrador
            if (!_context.Usuarios.Any(u => u.Role == UserRole.SuperAdministrador))
            {
                var superAdmin = new SuperAdministrador
                {
                    UsuarioId = Guid.NewGuid(),
                    Nome = "SuperAdmin",
                    UserName = "superadmin",
                    Email = "superadmin@controlapp.com",
                    Senha = _cryptoSHA256.HashPassword("@Super123"),
                    Role = UserRole.SuperAdministrador,
                    Ativo = true,
                    TipoUsuario = "SuperAdministrador"
                };

                _context.Usuarios.Add(superAdmin);
            }

            // Criação do Visitante
            if (!_context.Usuarios.Any(u => u.Role == UserRole.Visitante))
            {
                var visitante = new Visitante
                {
                    UsuarioId = Guid.NewGuid(),
                    Nome = "Visitante",
                    UserName = "visitante",
                    Email = "visitante@controlapp.com",
                    Senha = _cryptoSHA256.HashPassword("@Visitante123"),
                    Role = UserRole.Visitante,
                    Ativo = true,
                    TipoUsuario = "Visitante"
                };

                _context.Usuarios.Add(visitante);
            }

            await _context.SaveChangesAsync();
        }
    }
}