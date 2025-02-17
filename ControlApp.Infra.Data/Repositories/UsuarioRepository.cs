using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;
using ControlApp.Domain.Interfaces.Repositories;
using ControlApp.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly DataContext _context;

    public UsuarioRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Usuario> CriarUsuarioAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario> ObterUsuarioPorIdAsync(Guid id)
    {
        return await _context.Usuarios
           .Where(u => u.Ativo)
           .FirstOrDefaultAsync(u => u.UsuarioId == id);
    }

    public async Task<Usuario> ObterUsuarioPorEmailAsync(string email)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AtualizarUsuarioAsync(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeletarUsuarioAsync(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            usuario.Ativo = false;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<Usuario> ObterUsuarioPorUserNameAsync(string userName)  // Implementação do novo método
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.UserName == userName && u.Ativo);
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        return await _context.Usuarios
       .Where(u => u.Ativo)
       .ToListAsync();
    }
    public async Task<Usuario> ObterUsuariosPorRoleAsync(UserRole role)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Role == role);  // Busca o primeiro usuário com o role especificado
    }
    public async Task<Usuario> ObterUsuarioPorCpfAsync(string cpf)
    {
        return await _context.Usuarios
            .OfType<Tecnico>()  // Filtrando para buscar apenas Técnicos
            .FirstOrDefaultAsync(u => u.Cpf == cpf); // Busca pelo CPF no Técnico
    }
}
