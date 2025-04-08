/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlApp.Domain.Entities;
using ControlApp.Infra.Data.Contexts;
using ControlApp.Infra.Data.MongoDB.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ControlApp.Infra.Data.Services
{
    public class DatabaseSyncService
    {
        private readonly DataContext _sqlContext;
        private readonly BaseRepository<Ponto> _pontosMongoRepository;
        private readonly BaseRepository<Trajeto> _trajetosMongoRepository;
        private readonly BaseRepository<Localizacao> _localizacoesMongoRepository;
        private readonly BaseRepository<Usuario> _usuariosMongoRepository;
        private readonly BaseRepository<Empresa> _empresasMongoRepository;
        private readonly ILogger<DatabaseSyncService> _logger;

        public DatabaseSyncService(
            DataContext sqlContext,
            BaseRepository<Ponto> pontosMongoRepository,
            BaseRepository<Trajeto> trajetosMongoRepository,
            BaseRepository<Localizacao> localizacoesMongoRepository,
            BaseRepository<Usuario> usuariosMongoRepository,
            BaseRepository<Empresa> empresasMongoRepository,
            ILogger<DatabaseSyncService> logger)
        {
            _sqlContext = sqlContext;
            _pontosMongoRepository = pontosMongoRepository;
            _trajetosMongoRepository = trajetosMongoRepository;
            _localizacoesMongoRepository = localizacoesMongoRepository;
            _usuariosMongoRepository = usuariosMongoRepository;
            _empresasMongoRepository = empresasMongoRepository;
            _logger = logger;
        }

        public async Task SincronizarPontosAsync()
        {
            try
            {
                // Busca pontos não sincronizados no SQL Server
                var pontos = await _sqlContext.Pontos
                    .Where(p => p.Ativo)
                    .ToListAsync();

                foreach (var ponto in pontos)
                {
                    // Verifica se já existe no MongoDB
                    var pontoMongo = await _pontosMongoRepository.ObterPorIdAsync(ponto.Id);

                    if (pontoMongo == null)
                    {
                        // Cria no MongoDB se não existir
                        await _pontosMongoRepository.CriarAsync(ponto);
                        _logger.LogInformation($"Ponto {ponto.Id} sincronizado para MongoDB");
                    }
                    else
                    {
                        // Atualiza se já existir
                        await _pontosMongoRepository.AtualizarAsync(ponto.Id, ponto);
                        _logger.LogInformation($"Ponto {ponto.Id} atualizado no MongoDB");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar pontos");
                throw;
            }
        }

        public async Task SincronizarTrajetosAsync()
        {
            try
            {
                var trajetos = await _sqlContext.Trajetos
                    .Include(t => t.Localizacoes)
                    .ToListAsync();

                foreach (var trajeto in trajetos)
                {
                    var trajetoMongo = await _trajetosMongoRepository.ObterPorIdAsync(trajeto.Id);

                    if (trajetoMongo == null)
                    {
                        await _trajetosMongoRepository.CriarAsync(trajeto);
                        _logger.LogInformation($"Trajeto {trajeto.Id} sincronizado para MongoDB");

                        // Sincroniza localizações
                        foreach (var localizacao in trajeto.Localizacoes)
                        {
                            await _localizacoesMongoRepository.CriarAsync(localizacao);
                        }
                    }
                    else
                    {
                        await _trajetosMongoRepository.AtualizarAsync(trajeto.Id, trajeto);
                        _logger.LogInformation($"Trajeto {trajeto.Id} atualizado no MongoDB");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar trajetos");
                throw;
            }
        }

        public async Task SincronizarUsuariosAsync()
        {
            try
            {
                var usuarios = await _sqlContext.Usuarios
                    .Where(u => u.Ativo)
                    .ToListAsync();

                foreach (var usuario in usuarios)
                {
                    var usuarioMongo = await _usuariosMongoRepository.ObterPorIdAsync(usuario.UsuarioId);

                    if (usuarioMongo == null)
                    {
                        await _usuariosMongoRepository.CriarAsync(usuario);
                        _logger.LogInformation($"Usuário {usuario.UsuarioId} sincronizado para MongoDB");
                    }
                    else
                    {
                        await _usuariosMongoRepository.AtualizarAsync(usuario.UsuarioId, usuario);
                        _logger.LogInformation($"Usuário {usuario.UsuarioId} atualizado no MongoDB");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar usuários");
                throw;
            }
        }

        public async Task SincronizarEmpresasAsync()
        {
            try
            {
                var empresas = await _sqlContext.Empresas
                    .Include(e => e.Tecnicos)
                    .Include(e => e.Endereco)
                    .Where(e => e.Ativo)
                    .ToListAsync();

                foreach (var empresa in empresas)
                {
                    var empresaMongo = await _empresasMongoRepository.ObterPorIdAsync(empresa.EmpresaId);

                    if (empresaMongo == null)
                    {
                        await _empresasMongoRepository.CriarAsync(empresa);
                        _logger.LogInformation($"Empresa {empresa.EmpresaId} sincronizada para MongoDB");
                    }
                    else
                    {
                        await _empresasMongoRepository.AtualizarAsync(empresa.EmpresaId, empresa);
                        _logger.LogInformation($"Empresa {empresa.EmpresaId} atualizada no MongoDB");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar empresas");
                throw;
            }
        }

        // Método principal para sincronização completa
        public async Task SincronizarTodosAsync()
        {
            await SincronizarPontosAsync();
            await SincronizarTrajetosAsync();
            await SincronizarUsuariosAsync();
            await SincronizarEmpresasAsync();
        }
    }
}*/