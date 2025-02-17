using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Dtos.Request;
using ControlApp.Domain.Dtos.Response;
using ControlApp.Domain.Entities;

namespace ControlApp.Domain.Interfaces.Services
{
    public interface IUsuarioService : IBaseService<Usuario>
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task ChangePasswordAsync(Guid usuarioId, string novaSenha);
        Task<CriarUsuarioResponseDto> CreateUsuarioAsync(CriarUsuarioRequestDto requestDto);
        Task<AtualizarUsuarioResponseDto> UpdateUsuarioAsync(Guid usuarioId, AtualizarUsuarioRequestDto requestDto);
        Task<AutenticarUsuarioResponseDto> AuthenticateUsuarioAsync(AutenticarUsuarioRequestDto requestDto);
        Task AtualizarLocalizacaoAtualAsync(Guid usuarioId, string latitude, string longitude);
        Task<UsuarioResponseDto?> GetByIdAsync(Guid id);
    }
}