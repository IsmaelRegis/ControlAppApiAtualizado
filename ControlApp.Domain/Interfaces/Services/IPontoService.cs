using ControlApp.Domain.Dtos.Request;
using ControlApp.Domain.Dtos.Response;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public interface IPontoService : IBaseService<Ponto>
{
    Task<RegistrarInicioExpedienteResponseDto> RegisterInicioExpedienteAsync(Guid usuarioId, RegistrarInicioExpedienteRequestDto dto);
    Task<RegistrarFimExpedienteResponseDto> RegisterFimExpedienteAsync(Guid usuarioId, Guid pontoId, RegistrarFimExpedienteRequestDto dto);
    Task<RegistrarInicioPausaResponseDto> RegisterInicioPausaAsync(Guid usuarioId, RegistrarInicioPausaRequestDto dto);
    Task<RegistrarFimPausaResponseDto> RegisterFimPausaAsync(Guid usuarioId, Guid pontoId, RegistrarFimPausaRequestDto dto);
    Task<List<TecnicoResponseDto>> GetPontosComTrajetosAsync();
    Task<ConsultarPontoResponseDto> GetPontoByIdAsync(Guid usuarioId);
    Task<ConsultarTecnicoResponseDto> GetTecnicoComPontosAsync(Guid usuarioId);
    Task<List<PontoCombinadoResponseDto>> GetAllPontosCombinadoAsync();
    Task<List<PontoCombinadoResponseDto>> GetPontosCombinadoPorUsuarioIdAsync(Guid usuarioId);
}
