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
    public class LocalizacaoRepository : ILocalizacaoRepository
    {
        private readonly DataContext _context;
        //private readonly BaseRepository<Localizacao> _mongoRepository;

        /*public LocalizacaoRepository(
            DataContext context,
            BaseRepository<Localizacao> mongoRepository)
        {
            _context = context;
            _mongoRepository = mongoRepository;
        }*/

        public LocalizacaoRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Localizacao?> GetByIdAsync(Guid id)
        {
            /*// Primeiro tenta buscar no MongoDB
            var localizacaoMongo = await _mongoRepository.ObterPorIdAsync(id);
            if (localizacaoMongo != null) return localizacaoMongo;*/

            // Busca no SQL Server
            return await _context.Localizacoes.FindAsync(id);
        }

        public async Task<IEnumerable<Localizacao>> ObterLocalizacoesPorTrajetoIdAsync(Guid trajetoId)
        {
            /*// Tenta buscar no MongoDB primeiro
            var localizacoesMongo = await _mongoRepository.ObterPorFiltroAsync(
                l => l.TrajetoId == trajetoId
            );
            
            // Se encontrar no MongoDB, retorna
            if (localizacoesMongo.Any()) return localizacoesMongo;*/

            // Busca no SQL Server
            return await _context.Localizacoes
                .Where(l => l.TrajetoId == trajetoId)
                .ToListAsync();
        }

        public async Task<bool> AdicionarLocalizacaoAsync(Localizacao localizacao)
        {
            try
            {
                /*// Opção de salvar no MongoDB
                // await _mongoRepository.AdicionarAsync(localizacao);*/

                // Salva no SQL Server
                await _context.Localizacoes.AddAsync(localizacao);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ExcluirLocalizacoesPorTrajetoIdAsync(Guid trajetoId)
        {
            try
            {
                // Implementação dependendo do seu ORM (Entity Framework, Dapper, etc.)
                // Por exemplo, com Entity Framework:
                var localizacoes = await _context.Localizacoes
                    .Where(l => l.TrajetoId == trajetoId)
                    .ToListAsync();

                _context.Localizacoes.RemoveRange(localizacoes);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}