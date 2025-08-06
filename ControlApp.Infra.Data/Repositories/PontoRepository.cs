using ControlApp.Domain.Dtos.Response;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Infra.Data.Contexts;
//using ControlApp.Infra.Data.MongoDB.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using MongoDB.Driver;

namespace ControlApp.Infra.Data.Repositories
{
    public class PontoRepository : IPontoRepository
    {
        private readonly DataContext _context;
        private readonly ILocalizacaoRepository _localizacaoRepository;
        //private readonly BaseRepository<Ponto> _mongoRepository;

        /*public PontoRepository(
            DataContext context,
            ILocalizacaoRepository localizacaoRepository,
            BaseRepository<Ponto> mongoRepository)
        {
            _context = context;
            _localizacaoRepository = localizacaoRepository;
            _mongoRepository = mongoRepository;
        }*/

        public PontoRepository(
            DataContext context,
            ILocalizacaoRepository localizacaoRepository)
        {
            _context = context;
            _localizacaoRepository = localizacaoRepository;
        }

        public async Task<List<Ponto>> ObterPausasPorExpedienteIdAsync(Guid expedienteId)
        {
            return await _context.Pontos
                .Where(p => p.TipoPonto == TipoPonto.Pausa &&
                            p.ExpedienteId == expedienteId &&
                            p.Ativo)
                .ToListAsync();
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

                /*// Atualiza também no MongoDB
                await _mongoRepository.AtualizarAsync(id, pontoAtualizado);*/
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
            // Salva no SQL Server
            await _context.Pontos.AddAsync(ponto);
            await _context.SaveChangesAsync();

            /*// Salva no MongoDB
            await _mongoRepository.CriarAsync(ponto);*/

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

                /*// Atualiza no MongoDB
                await _mongoRepository.AtualizarAsync(id, ponto);*/
            }
        }

        public async Task<Ponto?> ObterPontoPorIdAsync(Guid id)
        {
            /*// Primeiro tenta buscar no MongoDB
            var pontoMongo = await _mongoRepository.ObterPorIdAsync(id);
            if (pontoMongo != null) return pontoMongo;*/

            // Busca no SQL Server
            return await _context.Pontos.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<ICollection<Ponto>> ObterPontosPorPeriodoAsync(DateTime inicio, DateTime fim)
        {
            /*// Tenta buscar no MongoDB primeiro
            var pontosMongo = await _mongoRepository.ObterPorFiltroAsync(
                p => p.InicioExpediente >= inicio && p.FimExpediente <= fim
            );

            // Se encontrar no MongoDB, retorna
            if (pontosMongo.Any()) return pontosMongo.ToList();*/

            // Busca no SQL Server
            return await _context.Pontos
                .Where(p => p.InicioExpediente >= inicio && p.FimExpediente <= fim)
                .ToListAsync();
        }

        public async Task<ICollection<Ponto>> ObterPontosPorTecnicoIdAsync(Guid usuarioId)
        {
            /*// Tenta buscar no MongoDB primeiro
            var pontosMongo = await _mongoRepository.ObterPorFiltroAsync(
                p => p.UsuarioId == usuarioId
            );

            // Se encontrar no MongoDB, retorna
            if (pontosMongo.Any()) return pontosMongo.ToList();*/

            // Busca no SQL Server
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

                /*// Atualiza o ponto no MongoDB
                await _mongoRepository.AtualizarAsync(pontoId, ponto);*/
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

                /*// Atualiza o ponto no MongoDB
                await _mongoRepository.AtualizarAsync(pontoId, ponto);*/
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ICollection<Ponto>> ObterPontosPorTipoAsync(Guid usuarioId, TipoPonto tipoPonto)
        {
            /*// Tenta buscar no MongoDB primeiro
            var pontosMongo = await _mongoRepository.ObterPorFiltroAsync(
                p => p.UsuarioId == usuarioId && p.TipoPonto == tipoPonto
            );

            // Se encontrar no MongoDB, retorna
            if (pontosMongo.Any()) return pontosMongo.ToList();*/

            // Busca no SQL Server
            return await _context.Pontos
                .Where(p => p.UsuarioId == usuarioId && p.TipoPonto == tipoPonto)
                .ToListAsync();
        }

        public async Task<ICollection<Ponto>> ObterPontoPorUsuarioId(Guid usuarioId)
        {
            /*// Tenta buscar no MongoDB primeiro
            var pontosMongo = await _mongoRepository.ObterPorFiltroAsync(
                p => p.UsuarioId == usuarioId && p.Ativo
            );

            // Se encontrar no MongoDB, retorna
            if (pontosMongo.Any()) return pontosMongo.ToList();*/

            // Busca no SQL Server
            return await _context.Pontos
                .Where(p => p.UsuarioId == usuarioId && p.Ativo)
                .ToListAsync();
        }

        public async Task<ICollection<Ponto>> ObterPontosPorUsuarioIdETipo(Guid usuarioId, TipoPonto tipoPonto)
        {
            /*// Tenta buscar no MongoDB primeiro
            var pontosMongo = await _mongoRepository.ObterPorFiltroAsync(
                p => p.UsuarioId == usuarioId && p.TipoPonto == tipoPonto
            );

            // Se encontrar no MongoDB, retorna
            if (pontosMongo.Any()) return pontosMongo.ToList();*/

            // Busca no SQL Server
            return await _context.Pontos
                .Where(p => p.UsuarioId == usuarioId && p.TipoPonto == tipoPonto)
                .ToListAsync();
        }

        public async Task<ICollection<Ponto>> ObterPontosPorUsuarioEPeriodoAsync(Guid usuarioId, DateTime inicio, DateTime fim)
        {
            /*// Tenta buscar no MongoDB primeiro
            var pontosMongo = await _mongoRepository.ObterPorFiltroAsync(
                p => p.UsuarioId == usuarioId && p.InicioExpediente >= inicio && p.InicioExpediente < fim
            );

            // Se encontrar no MongoDB, retorna
            if (pontosMongo.Any()) return pontosMongo.ToList();*/

            // Busca no SQL Server
            return await _context.Pontos
                .Where(p => p.UsuarioId == usuarioId && p.InicioExpediente >= inicio && p.InicioExpediente < fim)
                .ToListAsync();
        }

        public async Task<ICollection<Ponto>> ObterTodosPontosAsync()
        {
            /*// Tenta buscar no MongoDB primeiro
            var pontosMongo = await _mongoRepository.ObterTodosAsync();

            // Se encontrar no MongoDB, retorna
            if (pontosMongo.Any()) return pontosMongo.ToList();*/

            // Busca no SQL Server
            return await _context.Set<Ponto>()
                .Include(p => p.Tecnico) // Garante que o técnico seja carregado
                .ToListAsync();
        }

        public async Task<IEnumerable<Ponto>> ObterPontosPorUsuarioNaDataAsync(Guid usuarioId, DateTime data)
        {
            var dataInicio = data.Date;
            var dataFim = data.Date.AddDays(1);

            return await _context.Pontos
                .AsNoTracking()
                .Where(p => p.UsuarioId == usuarioId &&
                       (
                           (p.InicioExpediente >= dataInicio && p.InicioExpediente < dataFim) ||
                           (p.InicioPausa >= dataInicio && p.InicioPausa < dataFim)
                       )
                )
                .OrderBy(p => p.InicioExpediente ?? p.InicioPausa)
                .ToListAsync();
        }


    }
}