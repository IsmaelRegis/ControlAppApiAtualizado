using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly DataContext _context;
    /* private readonly BaseRepository<Usuario> _mongoRepository; */

    /*
    public UsuarioRepository(
        DataContext context,
        BaseRepository<Usuario> mongoRepository)
    {
        _context = context;
        _mongoRepository = mongoRepository;
    }
    */

    // Construtor corrigido sem MongoDB
    public UsuarioRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Usuario> CriarUsuarioAsync(Usuario usuario)
    {
        // Salva no SQL Server
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();

        /* // Salva no MongoDB
        await _mongoRepository.CriarAsync(usuario); */

        return usuario;
    }

    public async Task<Usuario> ObterUsuarioPorIdAsync(Guid id)
    {
        /* // Primeiro tenta buscar no MongoDB
        var usuarioMongo = await _mongoRepository.ObterPorIdAsync(id);

        // Verifica se o usuário está ativo
        if (usuarioMongo != null && usuarioMongo.Ativo)
        {
            return usuarioMongo;
        } */

        // Busca no SQL Server
        return await _context.Usuarios
           .Where(u => u.Ativo)
           .FirstOrDefaultAsync(u => u.UsuarioId == id);
    }

    public async Task<Usuario> ObterUsuarioPorEmailAsync(string email)
    {
        /* // Tenta buscar no MongoDB primeiro
        var usuariosMongo = await _mongoRepository.ObterPorFiltroAsync(
            u => u.Email == email
        );

        // Se encontrar no MongoDB, retorna
        var usuarioMongo = usuariosMongo.FirstOrDefault();
        if (usuarioMongo != null) return usuarioMongo; */

        // Busca no SQL Server
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AtualizarUsuarioAsync(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            // Atualiza no SQL Server
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            /* // Atualiza no MongoDB
            await _mongoRepository.AtualizarAsync(id, usuario); */
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

            /* // Atualiza no MongoDB
            await _mongoRepository.AtualizarAsync(id, usuario); */
        }
    }

    public async Task<Usuario> ObterUsuarioPorUserNameAsync(string userName)
    {
        /* // Tenta buscar no MongoDB primeiro
        var usuariosMongo = await _mongoRepository.ObterPorFiltroAsync(
            u => u.UserName == userName && u.Ativo
        );

        // Se encontrar no MongoDB, retorna
        var usuarioMongo = usuariosMongo.FirstOrDefault();
        if (usuarioMongo != null) return usuarioMongo; */

        // Busca no SQL Server
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.UserName == userName && u.Ativo);
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        /* // Tenta buscar no MongoDB primeiro
        var usuariosMongo = await _mongoRepository.ObterPorFiltroAsync(u => u.Ativo);

        // Se encontrar no MongoDB, retorna
        if (usuariosMongo.Any()) return usuariosMongo; */

        // Busca no SQL Server
        return await _context.Usuarios
            .Where(u => u.Ativo)
            .ToListAsync();
    }

    public async Task<Usuario> ObterUsuariosPorRoleAsync(UserRole role)
    {
        /* // Tenta buscar no MongoDB primeiro
        var usuariosMongo = await _mongoRepository.ObterPorFiltroAsync(
            u => u.Role == role
        );

        // Se encontrar no MongoDB, retorna o primeiro
        var usuarioMongo = usuariosMongo.FirstOrDefault();
        if (usuarioMongo != null) return usuarioMongo; */

        // Busca no SQL Server
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Role == role);
    }

    public async Task<Usuario> ObterUsuarioPorCpfAsync(string cpf)
    {
        /* // Tenta buscar no MongoDB primeiro
        var usuariosMongo = await _mongoRepository.ObterPorFiltroAsync(
            u => u is Tecnico && ((Tecnico)u).Cpf == cpf
        );

        // Se encontrar no MongoDB, retorna o primeiro
        var usuarioMongo = usuariosMongo.FirstOrDefault();
        if (usuarioMongo != null) return usuarioMongo; */

        // Busca no SQL Server
        return await _context.Usuarios
            .OfType<Tecnico>()
            .FirstOrDefaultAsync(u => u.Cpf == cpf);
    }
}