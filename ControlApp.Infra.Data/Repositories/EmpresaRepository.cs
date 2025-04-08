using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Infra.Data.Contexts;
//using ControlApp.Infra.Data.MongoDB.Repositories;
using Microsoft.EntityFrameworkCore;
namespace ControlApp.Infra.Data.Repositories
{
    public class EmpresaRepository : IEmpresaRepository
    {
        private readonly DataContext _context;
        //private readonly BaseRepository<Empresa> _mongoRepository;

        /*public EmpresaRepository(
            DataContext context,
            BaseRepository<Empresa> mongoRepository)
        {
            _context = context;
            _mongoRepository = mongoRepository;
        }*/

        public EmpresaRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Empresa> CriarEmpresaAsync(Empresa empresa)
        {
            // Salva no SQL Server
            await _context.Empresas.AddAsync(empresa);
            await _context.SaveChangesAsync();

            /*// Salva no MongoDB
            await _mongoRepository.CriarAsync(empresa);*/

            return empresa;
        }

        public async Task<Empresa?> ObterEmpresaPorIdAsync(Guid id)
        {
            /*// Primeiro tenta buscar no MongoDB
            var empresaMongo = await _mongoRepository.ObterPorIdAsync(id);
            if (empresaMongo != null)
            {
                // Se encontrar no MongoDB, verifica se está ativo
                return empresaMongo.Ativo ? empresaMongo : null;
            }*/

            // Busca no SQL Server com includes
            return await _context.Empresas
                .Include(e => e.Tecnicos)
                .Include(e => e.Endereco)
                .FirstOrDefaultAsync(e => e.EmpresaId == id && e.Ativo);
        }

        public async Task<IEnumerable<Empresa>> GetAllEmpresasAsync()
        {
            /*// Tenta buscar no MongoDB primeiro
            var empresasMongo = await _mongoRepository.ObterPorFiltroAsync(e => e.Ativo);
            
            // Se encontrar no MongoDB com empresas ativas, retorna
            if (empresasMongo.Any()) return empresasMongo;*/

            // Busca no SQL Server com includes
            return await _context.Empresas
                .Include(e => e.Tecnicos)
                .Include(e => e.Endereco)
                .Where(e => e.Ativo)
                .ToListAsync();
        }

        public async Task AtualizarEmpresaAsync(Empresa empresa)
        {
            // Atualiza no SQL Server
            _context.Empresas.Update(empresa);
            await _context.SaveChangesAsync();

            /*// Atualiza no MongoDB
            await _mongoRepository.AtualizarAsync(empresa.EmpresaId, empresa);*/
        }

        public async Task ExcluirEmpresaAsync(Guid id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa != null)
            {
                // Desativa no SQL Server
                empresa.Ativo = false;
                _context.Empresas.Update(empresa);
                await _context.SaveChangesAsync();

                /*// Atualiza no MongoDB
                await _mongoRepository.AtualizarAsync(id, empresa);*/
            }
        }
    }
}