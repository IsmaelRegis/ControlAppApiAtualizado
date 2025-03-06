using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ControlApp.Infra.Data.Repositories
{
    public class EmpresaRepository : IEmpresaRepository
    {
        private readonly DataContext _context;

        public EmpresaRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Empresa> CriarEmpresaAsync(Empresa empresa)
        {
            await _context.Empresas.AddAsync(empresa);
            await _context.SaveChangesAsync();
            return empresa;
        }

        public async Task<Empresa?> ObterEmpresaPorIdAsync(Guid id)
        {
            return await _context.Empresas
                .Include(e => e.Tecnicos)
                .Include(e => e.Endereco) // Adicionado para carregar o Endereco
                .FirstOrDefaultAsync(e => e.EmpresaId == id && e.Ativo);
        }
        public async Task<IEnumerable<Empresa>> GetAllEmpresasAsync()
        {
            return await _context.Empresas
                .Include(e => e.Tecnicos)
                .Include(e => e.Endereco) // Adicionado
                .Where(e => e.Ativo)
                .ToListAsync();
        }

        public async Task AtualizarEmpresaAsync(Empresa empresa)
        {
            _context.Empresas.Update(empresa);
            await _context.SaveChangesAsync();
        }

        public async Task ExcluirEmpresaAsync(Guid id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa != null)
            {
                empresa.Ativo = false; 
                _context.Empresas.Update(empresa);
                await _context.SaveChangesAsync();
            }
        }
    }
}