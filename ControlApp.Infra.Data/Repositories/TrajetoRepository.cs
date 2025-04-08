using ControlApp.Domain.Entities;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Infra.Data.Contexts;
//using ControlApp.Infra.Data.MongoDB.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControlApp.Infra.Data.Repositories
{
    public class TrajetoRepository : ITrajetoRepository
    {
        private readonly DataContext _context;
        /*private readonly BaseRepository<Trajeto> _mongoRepository;
        private readonly BaseRepository<Localizacao> _localizacaoMongoRepository;*/

        /*public TrajetoRepository(
            DataContext context,
            BaseRepository<Trajeto> mongoRepository,
            BaseRepository<Localizacao> localizacaoMongoRepository)
        {
            _context = context;
            _mongoRepository = mongoRepository;
            _localizacaoMongoRepository = localizacaoMongoRepository;
        }*/

        public TrajetoRepository(DataContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Trajeto trajeto)
        {
            // Salva no SQL Server
            _context.Trajetos.Add(trajeto);
            await _context.SaveChangesAsync();

            /*// Salva no MongoDB
            await _mongoRepository.CriarAsync(trajeto);*/
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

                // Adiciona o trajeto no SQL Server
                await _context.Trajetos.AddAsync(trajeto);
                await _context.SaveChangesAsync();

                // Adiciona localizações no SQL Server
                foreach (var localizacao in localizacoes)
                {
                    localizacao.TrajetoId = trajeto.Id;
                    _context.Localizacoes.Add(localizacao);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                /*// Salva trajeto e localizações no MongoDB
                await _mongoRepository.CriarAsync(trajeto);
                foreach (var localizacao in localizacoes)
                {
                    await _localizacaoMongoRepository.CriarAsync(localizacao);
                }*/
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Trajeto>> GetAllAsync()
        {
            /*// Tenta buscar no MongoDB primeiro
            var trajetosMongo = await _mongoRepository.ObterTodosAsync();

            // Se encontrar no MongoDB com localizações, retorna
            if (trajetosMongo.Any())
            {
                // Como o MongoDB pode não ter o Include, pode ser necessário buscar localizações separadamente
                var trajetosComLocalizacoes = new List<Trajeto>();
                foreach (var trajeto in trajetosMongo)
                {
                    var localizacoes = await _localizacaoMongoRepository.ObterPorFiltroAsync(
                        l => l.TrajetoId == trajeto.Id
                    );
                    trajeto.Localizacoes = localizacoes.ToList();
                    trajetosComLocalizacoes.Add(trajeto);
                }
                return trajetosComLocalizacoes;
            }*/

            // Busca no SQL Server
            return await _context.Trajetos
                .Include(t => t.Localizacoes)
                .ToListAsync();
        }

        public async Task<Trajeto?> GetByIdAsync(Guid id)
        {
            /*// Primeiro tenta buscar no MongoDB
            var trajetoMongo = await _mongoRepository.ObterPorIdAsync(id);
            if (trajetoMongo != null)
            {
                // Busca localizações no MongoDB
                var localizacoes = await _localizacaoMongoRepository.ObterPorFiltroAsync(
                    l => l.TrajetoId == id
                );
                trajetoMongo.Localizacoes = localizacoes.ToList();
                return trajetoMongo;
            }*/

            // Busca no SQL Server
            return await _context.Trajetos
                .Include(t => t.Localizacoes)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Trajeto?> GetTrajetoDoDiaAsync(Guid usuarioId, DateTime date)
        {
            /*// Tenta buscar no MongoDB primeiro
            var trajetosMongo = await _mongoRepository.ObterPorFiltroAsync(
                t => t.UsuarioId == usuarioId && t.Data.Date == date.Date
            );

            var trajetoMongo = trajetosMongo.FirstOrDefault();
            if (trajetoMongo != null)
            {
                // Busca localizações no MongoDB
                var localizacoes = await _localizacaoMongoRepository.ObterPorFiltroAsync(
                    l => l.TrajetoId == trajetoMongo.Id
                );
                trajetoMongo.Localizacoes = localizacoes.ToList();
                return trajetoMongo;
            }*/

            // Busca no SQL Server
            return await _context.Trajetos
                .Include(t => t.Localizacoes)
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Data.Date == date.Date);
        }

        public async Task<IEnumerable<Trajeto>> ObterTrajetosPorUsuarioAsync(Guid usuarioId)
        {
            /*// Tenta buscar no MongoDB primeiro
            var trajetosMongo = await _mongoRepository.ObterPorFiltroAsync(
                t => t.UsuarioId == usuarioId
            );

            // Se encontrar, adiciona localizações
            if (trajetosMongo.Any())
            {
                var trajetosComLocalizacoes = new List<Trajeto>();
                foreach (var trajeto in trajetosMongo)
                {
                    var localizacoes = await _localizacaoMongoRepository.ObterPorFiltroAsync(
                        l => l.TrajetoId == trajeto.Id
                    );
                    trajeto.Localizacoes = localizacoes.ToList();
                    trajetosComLocalizacoes.Add(trajeto);
                }
                return trajetosComLocalizacoes;
            }*/

            // Busca no SQL Server
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

        public async Task UpdateAsync(Trajeto trajeto)
        {
            // Atualiza no SQL Server
            var trajetoExistente = await _context.Trajetos
                .Include(t => t.Localizacoes)
                .FirstOrDefaultAsync(t => t.Id == trajeto.Id);

            if (trajetoExistente == null)
            {
                throw new Exception("Trajeto não encontrado.");
            }

            // Atualizar os dados do trajeto
            trajetoExistente.Status = trajeto.Status;
            trajetoExistente.Data = trajeto.Data;
            trajetoExistente.DistanciaTotalKm = trajeto.DistanciaTotalKm;
            trajetoExistente.DuracaoTotal = trajeto.DuracaoTotal;

            // Atualiza localizações
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
                        trajetoExistente.Localizacoes.Add(localizacao);
                    }
                }
            }

            // Salva no SQL Server
            await _context.SaveChangesAsync();

            /*// Atualiza no MongoDB
            await _mongoRepository.AtualizarAsync(trajeto.Id, trajeto);

            // Atualiza localizações no MongoDB
            foreach (var localizacao in trajeto.Localizacoes)
            {
                await _localizacaoMongoRepository.AtualizarAsync(localizacao.LocalizacaoId, localizacao);
            }*/
        }
    }
}