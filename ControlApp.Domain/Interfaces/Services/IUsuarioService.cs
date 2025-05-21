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
        Task<AutenticarUsuarioResponseDto> AuthenticateUsuarioAsync(AutenticarUsuarioRequestDto requestDto,string deviceInfo = null,string audience = "VibeService");
        Task LogoutUsuarioAsync(Guid usuarioId, string token);
        Task AtualizarLocalizacaoAtualAsync(Guid usuarioId, string latitude, string longitude);
        Task<bool> AdicionarRegistroLocalizacaoAsync(Guid usuarioId, string latitude, string longitude);
        Task<UsuarioResponseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<UsuarioResponseDto>> GetAllTecnicosAsync();
        Task<EmpresaResponseDto> CreateEmpresaAsync(CriarEmpresaRequestDto requestDto);
        Task<EmpresaResponseDto?> GetEmpresaByIdAsync(Guid id);
        Task<IEnumerable<EmpresaResponseDto>> GetAllEmpresasAsync();
        Task<EmpresaResponseDto> UpdateEmpresaAsync(Guid id, AtualizarEmpresaRequestDto requestDto);
        Task DeleteEmpresaAsync(Guid id);
    }
}