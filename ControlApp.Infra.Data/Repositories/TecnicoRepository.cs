using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Infra.Data.Contexts;
//using ControlApp.Infra.Data.MongoDB.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControlApp.Infra.Data.Repositories
{
    public class TecnicoRepository : ITecnicoRepository
    {
        private readonly DataContext _context;
        /*private readonly BaseRepository<Usuario> _mongoRepository;
        private readonly BaseRepository<Ponto> _pontosMongoRepository;*/

        /*public TecnicoRepository(
            DataContext context,
            BaseRepository<Usuario> mongoRepository,
            BaseRepository<Ponto> pontosMongoRepository)
        {
            _context = context;
            _mongoRepository = mongoRepository;
            _pontosMongoRepository = pontosMongoRepository;
        }*/

        public TecnicoRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Usuario> ObterUsuarioPorEmailAsync(string email)
        {
            /*// Primeiro tenta buscar no MongoDB
            var usuarioMongo = await _mongoRepository.ObterPorFiltroAsync(
                u => u.Email == email
            );

            // Se encontrar no MongoDB, retorna
            var usuario = usuarioMongo.FirstOrDefault();
            if (usuario != null) return usuario;*/

            // Busca no SQL Server
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario> ObterUsuarioPorIdAsync(Guid id)
        {
            /*// Primeiro tenta buscar no MongoDB
            var usuarioMongo = await _mongoRepository.ObterPorIdAsync(id);
            if (usuarioMongo != null) return usuarioMongo;*/

            // Busca no SQL Server
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == id);
        }

        public async Task<Usuario> CriarUsuarioAsync(Usuario usuario)
        {
            // Salva no SQL Server
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();

            /*// Salva no MongoDB
            await _mongoRepository.CriarAsync(usuario);*/

            return usuario;
        }

        public async Task AtualizarUsuarioAsync(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                // Atualiza no SQL Server
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                /*// Atualiza no MongoDB
                await _mongoRepository.AtualizarAsync(id, usuario);*/
            }
        }

        public async Task DeletarUsuarioAsync(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                // Desativa no SQL Server
                usuario.Ativo = false;
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                /*// Atualiza no MongoDB
                await _mongoRepository.AtualizarAsync(id, usuario);*/
            }
        }

        public async Task<ICollection<Ponto>> ObterPontosPorTecnicoAsync(Guid usuarioId)
        {
            /*// Tenta buscar no MongoDB primeiro
            var pontosMongo = await _pontosMongoRepository.ObterPorFiltroAsync(
                p => p.UsuarioId == usuarioId && p.Ativo
            );

            // Se encontrar no MongoDB, retorna
            if (pontosMongo.Any()) return pontosMongo.ToList();*/

            // Busca no SQL Server
            return await _context.Pontos
                .Where(p => p.UsuarioId == usuarioId && p.Ativo)
                .ToListAsync();
        }

        public async Task<ICollection<Ponto>> ObterPontosPorPeriodoAsync(Guid usuarioId, DateTime inicio, DateTime fim)
        {
            /*// Tenta buscar no MongoDB primeiro
            var pontosMongo = await _pontosMongoRepository.ObterPorFiltroAsync(
                p => p.UsuarioId == usuarioId &&
                     p.InicioExpediente >= inicio &&
                     p.InicioExpediente <= fim &&
                     p.Ativo
            );

            // Se encontrar no MongoDB, retorna
            if (pontosMongo.Any()) return pontosMongo.ToList();*/

            // Busca no SQL Server
            return await _context.Pontos
                .Where(p => p.UsuarioId == usuarioId &&
                            p.InicioExpediente >= inicio &&
                            p.InicioExpediente <= fim &&
                            p.Ativo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            /*// Tenta buscar no MongoDB primeiro
            var usuariosMongo = await _mongoRepository.ObterTodosAsync();

            // Se encontrar no MongoDB, retorna
            if (usuariosMongo.Any()) return usuariosMongo;*/

            // Busca no SQL Server
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<Usuario> ObterUsuariosPorRoleAsync(UserRole role)
        {
            /*// Tenta buscar no MongoDB primeiro
            var usuariosMongo = await _mongoRepository.ObterPorFiltroAsync(
                u => u.Role == role
            );

            // Se encontrar no MongoDB, retorna o primeiro
            var usuario = usuariosMongo.FirstOrDefault();
            if (usuario != null) return usuario;*/

            // Busca no SQL Server
            return await _context.Usuarios
                .Where(u => u.Role == role)
                .FirstOrDefaultAsync();
        }

        public async Task<Usuario> ObterUsuarioPorCpfAsync(string cpf)
        {
            /*// Tenta buscar no MongoDB primeiro
            var usuariosMongo = await _mongoRepository.ObterPorFiltroAsync(
                u => u is Tecnico && ((Tecnico)u).Cpf == cpf
            );

            // Se encontrar no MongoDB, retorna o primeiro
            var usuario = usuariosMongo.FirstOrDefault();
            if (usuario != null) return usuario;*/

            // Busca no SQL Server
            return await _context.Usuarios
                .OfType<Tecnico>()
                .FirstOrDefaultAsync(t => t.Cpf == cpf);
        }

        public async Task<Usuario> ObterUsuarioPorUserNameAsync(string userName)
        {
            /*// Tenta buscar no MongoDB primeiro
            var usuariosMongo = await _mongoRepository.ObterPorFiltroAsync(
                u => u.UserName == userName && u.Ativo
            );

            // Se encontrar no MongoDB, retorna o primeiro
            var usuario = usuariosMongo.FirstOrDefault();
            if (usuario != null) return usuario;*/

            // Busca no SQL Server
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UserName == userName && u.Ativo);
        }

        public async Task<int> ObterUltimaMatriculaAsync()
        {
            // Esta consulta é um pouco mais complexa para MongoDB
            // Pode ser necessário uma estratégia diferente
            var ultimaMatricula = await _context.Usuarios.OfType<Tecnico>()
                .OrderByDescending(t => t.NumeroMatricula)
                .Select(t => t.NumeroMatricula)
                .FirstOrDefaultAsync();

            return ultimaMatricula != null ? int.Parse(ultimaMatricula) : 0;
        }

        public Task AtualizarTodosOsUsuariosAsync(Usuario usuario)
        {
            throw new NotImplementedException();
        }

        public Task<List<Usuario>> GetUsersByIdsAsync(List<Guid> userIds)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUsersAsync(List<Usuario> users)
        {
            throw new NotImplementedException();
        }
    }
}