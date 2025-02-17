using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ControlApp.Infra.Data.Repositories
{
    public class TecnicoRepository : ITecnicoRepository
    {
        private readonly DataContext _context;

        public TecnicoRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Usuario> ObterUsuarioPorEmailAsync(string email)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario> ObterUsuarioPorIdAsync(Guid id)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == id);
        }

        public async Task<Usuario> CriarUsuarioAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task AtualizarUsuarioAsync(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();
            }
        }


        public async Task DeletarUsuarioAsync(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                usuario.Ativo = false;
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ICollection<Ponto>> ObterPontosPorTecnicoAsync(Guid usuarioId)
        {
            return await _context.Pontos
                .Where(p => p.UsuarioId == usuarioId && p.Ativo)
                .ToListAsync();
        }

        public async Task<ICollection<Ponto>> ObterPontosPorPeriodoAsync(Guid usuarioId, DateTime inicio, DateTime fim)
        {
            return await _context.Pontos
                .Where(p => p.UsuarioId == usuarioId && p.InicioExpediente >= inicio && p.InicioExpediente <= fim && p.Ativo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }
        public async Task<Usuario> ObterUsuariosPorRoleAsync(UserRole role)
        {
            return await _context.Usuarios
                .Where(u => u.Role == role)
                .FirstOrDefaultAsync(); 
        }

        public async Task<Usuario> ObterUsuarioPorCpfAsync(string cpf)
        {
            return await _context.Usuarios
                .OfType<Tecnico>()  // Apenas técnicos
                .FirstOrDefaultAsync(t => t.Cpf == cpf);
        }

        public Task<Usuario> ObterUsuarioPorUserNameAsync(string userName)
        {
            throw new NotImplementedException();
        }
    }
}
