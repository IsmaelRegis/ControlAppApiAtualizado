using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;

namespace ControlApp.Domain.Interfaces.Repositories
{

    public interface IUsuarioRepository
    {
        Task<Usuario> CriarUsuarioAsync(Usuario usuario);
        Task<Usuario> ObterUsuarioPorIdAsync(Guid id);
        Task<Usuario> ObterUsuarioPorEmailAsync(string email); 
        Task<Usuario> ObterUsuarioPorCpfAsync(string cpf);
        Task<Usuario> ObterUsuarioPorUserNameAsync(string userName);
        Task AtualizarUsuarioAsync(Guid id);
        Task DeletarUsuarioAsync(Guid id);
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<Usuario> ObterUsuariosPorRoleAsync(UserRole role);
    }

}
