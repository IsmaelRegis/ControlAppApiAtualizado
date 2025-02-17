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
    public class TrajetoRepository : ITrajetoRepository
    {
        private readonly DataContext _context;

        public TrajetoRepository(DataContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Trajeto trajeto)
        {
            _context.Trajetos.Add(trajeto);
            await _context.SaveChangesAsync();
        }

        public async Task AddTrajetoComLocalizacoesAsync(Trajeto trajeto, List<Localizacao> localizacoes)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Caso já esteja anexado, desanexa
                if (_context.Entry(trajeto).State != EntityState.Detached)
                {
                    _context.Entry(trajeto).State = EntityState.Detached;
                }

                // Adiciona o trajeto
                await _context.Trajetos.AddAsync(trajeto);
                await _context.SaveChangesAsync();

                // Adiciona localizações
                foreach (var localizacao in localizacoes)
                {
                    localizacao.TrajetoId = trajeto.Id;
                    _context.Localizacoes.Add(localizacao);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Trajeto>> GetAllAsync()
        {
            return await _context.Trajetos
                .Include(t => t.Localizacoes)
                .ToListAsync();
        }

        public async Task<Trajeto?> GetByIdAsync(Guid id)
        {
            return await _context.Trajetos
                .Include(t => t.Localizacoes)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Trajeto?> GetTrajetoDoDiaAsync(Guid usuarioId, DateTime date)
        {
            return await _context.Trajetos
                .Include(t => t.Localizacoes)
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Data.Date == date.Date);
        }

        public async Task<IEnumerable<Trajeto>> ObterTrajetosPorUsuarioAsync(Guid usuarioId)
        {
            return await _context.Trajetos
                .Include(t => t.Localizacoes)
                .Where(t => t.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public void DetachEntity<T>(T entity) where T : class
        {
            var entry = _context.Entry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Detached;
            }
        }

        // ================================
        //  Atualização Parcial (sem .Update e sem rowversion)
        public async Task UpdateAsync(Trajeto trajeto)
        {
            var trajetoExistente = await _context.Trajetos
                .Include(t => t.Localizacoes)  // Carregar as localizações se necessário
                .FirstOrDefaultAsync(t => t.Id == trajeto.Id);

            if (trajetoExistente == null)
            {
                throw new Exception("Trajeto não encontrado.");
            }

            // Remover qualquer verificação de RowVersion

            // Atualizar os dados do trajeto
            trajetoExistente.Status = trajeto.Status;
            trajetoExistente.Data = trajeto.Data;
            trajetoExistente.DistanciaTotalKm = trajeto.DistanciaTotalKm;
            trajetoExistente.DuracaoTotal = trajeto.DuracaoTotal;

            // Se necessário, atualize as localizações ou outras propriedades
            if (trajeto.Localizacoes.Any())
            {
                foreach (var localizacao in trajeto.Localizacoes)
                {
                    var localizacaoExistente = trajetoExistente.Localizacoes
                        .FirstOrDefault(l => l.LocalizacaoId == localizacao.LocalizacaoId);

                    if (localizacaoExistente != null)
                    {
                        localizacaoExistente.Latitude = localizacao.Latitude;
                        localizacaoExistente.Longitude = localizacao.Longitude;
                        localizacaoExistente.DataHora = localizacao.DataHora;
                    }
                    else
                    {
                        trajetoExistente.Localizacoes.Add(localizacao);  // Adicionar novas localizações, se necessário
                    }
                }
            }

            // Salvar as mudanças no banco de dados
            await _context.SaveChangesAsync();
        }


    }
}
