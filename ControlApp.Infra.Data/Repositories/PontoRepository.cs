using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ControlApp.Infra.Data.Repositories
{
    public class PontoRepository : IPontoRepository
    {
        private readonly DataContext _context;
        private readonly ILocalizacaoRepository _localizacaoRepository;

        public PontoRepository(DataContext context, ILocalizacaoRepository localizacaoRepository)
        {
            _context = context;
            _localizacaoRepository = localizacaoRepository;
        }
        public async Task AtualizarPontoAsync(Guid id, Ponto pontoAtualizado)
        {
            try
            {
                var pontoExistente = await _context.Pontos.FindAsync(id);

                if (pontoExistente == null)
                {
                    throw new Exception("Ponto não encontrado.");
                }

                // Atualiza as propriedades do ponto existente
                pontoExistente.InicioExpediente = pontoAtualizado.InicioExpediente;
                pontoExistente.FimExpediente = pontoAtualizado.FimExpediente;
                pontoExistente.InicioPausa = pontoAtualizado.InicioPausa;
                pontoExistente.RetornoPausa = pontoAtualizado.RetornoPausa;
                pontoExistente.HorasTrabalhadas = pontoAtualizado.HorasTrabalhadas;
                pontoExistente.HorasExtras = pontoAtualizado.HorasExtras;
                pontoExistente.HorasDevidas = pontoAtualizado.HorasDevidas;
                pontoExistente.Ativo = pontoAtualizado.Ativo;
                pontoExistente.UsuarioId = pontoAtualizado.UsuarioId;
                pontoExistente.FotoInicioExpediente = pontoAtualizado.FotoInicioExpediente ?? pontoExistente.FotoInicioExpediente;
                pontoExistente.FotoFimExpediente = pontoAtualizado.FotoFimExpediente ?? pontoExistente.FotoFimExpediente;

                // Salva as alterações no banco de dados
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Trata a exceção de concorrência
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync();

                if (databaseValues == null)
                {
                    throw new Exception("O ponto foi excluído por outro processo.");
                }
                else
                {
                    var databasePonto = (Ponto)databaseValues.ToObject();
                    throw new Exception("O ponto foi modificado por outro usuário. Favor revisar os dados.");
                }
            }
            catch (Exception ex)
            {
                // Trata outros erros
                throw new Exception("Ocorreu um erro ao tentar atualizar o ponto.", ex);
            }
        }

        public async Task<Ponto> CriarPontoAsync(Ponto ponto)
        {
            await _context.Pontos.AddAsync(ponto);
            await _context.SaveChangesAsync();
            return ponto;
        }

        public async Task DesativarPontoAsync(Guid id)
        {
            var ponto = await _context.Pontos.FindAsync(id);
            if (ponto != null)
            {
                ponto.Ativo = false;
                _context.Pontos.Update(ponto);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Ponto?> ObterPontoPorIdAsync(Guid id)
        {
            return await _context.Pontos.FirstOrDefaultAsync(p => p.Id == id);
        }


        public async Task<ICollection<Ponto>> ObterPontosPorPeriodoAsync(DateTime inicio, DateTime fim)
        {
            return await _context.Pontos
                .Where(p => p.InicioExpediente >= inicio && p.FimExpediente <= fim)
                .ToListAsync();
        }


        public async Task<ICollection<Ponto>> ObterPontosPorTecnicoIdAsync(Guid usuarioId)
        {
            return await _context.Pontos
                .Where(p => p.UsuarioId == usuarioId)
                .ToListAsync();
        }
        public async Task AdicionarLocalizacoesAoTrajetoAsync(Guid pontoId, List<Localizacao> localizacoes)
        {
            var ponto = await _context.Pontos.FindAsync(pontoId);
            if (ponto == null)
            {
                throw new Exception("Ponto não encontrado.");
            }

            // Verifica se o ponto tem um trajeto associado
            var trajeto = await _context.Trajetos
                .Include(t => t.Localizacoes)
                .FirstOrDefaultAsync(t => t.UsuarioId == ponto.UsuarioId && t.Data.Date == ponto.InicioExpediente);

            if (trajeto == null)
            {
                throw new Exception("Nenhum trajeto encontrado para o ponto.");
            }

            // Associa as localizações ao trajeto
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                foreach (var localizacao in localizacoes)
                {
                    localizacao.TrajetoId = trajeto.Id; // Relaciona a localização ao trajeto
                    _context.Localizacoes.Add(localizacao); // Adiciona as localizações
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AtualizarLocalizacoesDoPontoAsync(Guid pontoId, List<Localizacao> novasLocalizacoes)
        {
            // Busca o ponto e, a partir dele, obtemos o trajeto relacionado ao usuário
            var ponto = await _context.Pontos.FindAsync(pontoId);
            if (ponto == null)
            {
                throw new Exception("Ponto não encontrado.");
            }

            // Verifica se há um trajeto existente relacionado ao usuário
            var trajeto = await _context.Trajetos
                .Include(t => t.Localizacoes)
                .FirstOrDefaultAsync(t => t.UsuarioId == ponto.UsuarioId && t.Data.Date == ponto.InicioExpediente);

            if (trajeto == null)
            {
                throw new Exception("Nenhum trajeto relacionado ao ponto foi encontrado.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Remove localizações antigas
                _context.Localizacoes.RemoveRange(trajeto.Localizacoes);
                await _context.SaveChangesAsync();

                // Adiciona novas localizações
                foreach (var localizacao in novasLocalizacoes)
                {
                    localizacao.TrajetoId = trajeto.Id;  // Relaciona ao trajeto
                    await _context.Localizacoes.AddAsync(localizacao);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ICollection<Ponto>> ObterPontosPorTipoAsync(Guid usuarioId, TipoPonto tipoPonto)
        {
            return await _context.Pontos
                .Where(p => p.UsuarioId == usuarioId && p.TipoPonto == tipoPonto)
                .ToListAsync();
        }
        public async Task<ICollection<Ponto>> ObterPontoPorUsuarioId(Guid usuarioId)
        {
            return await _context.Pontos
                .Where(p => p.UsuarioId == usuarioId && p.Ativo) 
                .ToListAsync();
        }

        public async Task<ICollection<Ponto>> ObterPontosPorUsuarioIdETipo(Guid usuarioId, TipoPonto tipoPonto)
        {
            return await _context.Pontos
                .Where(p => p.UsuarioId == usuarioId && p.TipoPonto == tipoPonto)
                .ToListAsync();
        }

        public async Task<ICollection<Ponto>> ObterPontosPorUsuarioEPeriodoAsync(Guid usuarioId, DateTime inicio, DateTime fim)
        {
            return await _context.Pontos
                .Where(p => p.UsuarioId == usuarioId && p.InicioExpediente >= inicio && p.InicioExpediente < fim)
                .ToListAsync();
        }
        public async Task<ICollection<Ponto>> ObterTodosPontosAsync()
        {
            return await _context.Set<Ponto>()
                .Include(p => p.Tecnico) // Garante que o técnico seja carregado
                .ToListAsync();
        }

    }
}