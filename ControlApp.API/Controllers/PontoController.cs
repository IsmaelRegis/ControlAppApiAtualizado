using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControlApp.Domain.Dtos.Request;
using ControlApp.Domain.Dtos.Response;
using ControlApp.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class PontoController : ControllerBase
{
    private readonly IPontoService _pontoService;

    #region Construtor
    public PontoController(IPontoService pontoService)
    {
        _pontoService = pontoService; // Injeção de dependência do serviço de pontos
    }
    #endregion

    #region Endpoints
    /* 
     * Endpoints para gerenciamento de pontos, incluindo registros e consultas.
     */

    // Apenas colaboradores podem registrar o início de expediente
    [Authorize(Roles = "Colaborador")]
    [HttpPost("{usuarioId}/registrarInicioExpediente")]
    public async Task<ActionResult<RegistrarInicioExpedienteResponseDto>> RegisterInicioExpedienteAsync(
        Guid usuarioId,
        [FromForm] RegistrarInicioExpedienteRequestDto ponto)
    {
        try
        {
            var response = await _pontoService.RegisterInicioExpedienteAsync(usuarioId, ponto); // Registra o início do expediente
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message); // Retorna erro de operação inválida
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Erro interno do servidor: " + ex.Message); // Retorna um erro genérico
        }
    }

    // Apenas colaboradores podem registrar o fim de expediente
    [Authorize(Roles = "Colaborador")]
    [HttpPost("{usuarioId}/registrarFimExpediente/{pontoId}")]
    public async Task<ActionResult<RegistrarFimExpedienteResponseDto>> RegisterFimExpedienteAsync(
        Guid usuarioId,
        Guid pontoId,
        [FromForm] RegistrarFimExpedienteRequestDto ponto)
    {
        try
        {
            var response = await _pontoService.RegisterFimExpedienteAsync(usuarioId, pontoId, ponto); // Registra o fim do expediente
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Erro interno do servidor: " + ex.Message);
        }
    }

    // Apenas colaboradores podem registrar o início de pausa
    [Authorize(Roles = "Colaborador")]
    [HttpPost("{usuarioId}/registrarInicioPausa")]
    public async Task<ActionResult<RegistrarInicioPausaResponseDto>> RegisterInicioPausaAsync(
        Guid usuarioId,
        [FromBody] RegistrarInicioPausaRequestDto ponto)
    {
        try
        {
            var response = await _pontoService.RegisterInicioPausaAsync(usuarioId, ponto); // Registra o início da pausa
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Erro interno do servidor: " + ex.Message);
        }
    }

    // Apenas colaboradores podem registrar o fim de pausa
    [Authorize(Roles = "Colaborador")]
    [HttpPost("{usuarioId}/registrarFimPausa/{pontoId}")]
    public async Task<ActionResult> RegisterFimPausaAsync(
        Guid usuarioId,
        Guid pontoId,
        [FromBody] RegistrarFimPausaRequestDto ponto)
    {
        try
        {
            await _pontoService.RegisterFimPausaAsync(usuarioId, pontoId, ponto); // Registra o fim da pausa
            return Ok("Fim de pausa registrado com sucesso.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Erro interno do servidor: " + ex.Message);
        }
    }

    // Apenas administradores podem consultar os pontos e os trajetos
    [Authorize(Roles = "Administrador")]
    [HttpGet("pontos-trajetos")]
    public async Task<ActionResult<List<TecnicoResponseDto>>> GetPontosComTrajetosAsync()
    {
        try
        {
            var responseDtos = await _pontoService.GetPontosComTrajetosAsync(); // Consulta os pontos e os trajetos
            return Ok(responseDtos);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Erro interno do servidor: " + ex.Message);
        }
    }

    // Tanto os administradores quanto os colaboradores podem acessar esta rota
    [Authorize(Roles = "Administrador, Colaborador")]
    [HttpGet("{usuarioId}/tecnico-com-pontos")]
    public async Task<ActionResult<ConsultarTecnicoResponseDto>> GetTecnicoComPontos([FromRoute] Guid usuarioId)
    {
        try
        {
            var tecnicoComPontos = await _pontoService.GetTecnicoComPontosAsync(usuarioId); // Consulta técnico com seus pontos
            return Ok(tecnicoComPontos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    [HttpGet("pontos-combinados")]
    public async Task<ActionResult<List<PontoCombinadoResponseDto>>> GetAllPontosCombinadoAsync()
    {
        try
        {
            var pontosCombinados = await _pontoService.GetAllPontosCombinadoAsync(); // Consulta todos os pontos combinados
            return Ok(pontosCombinados);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    [HttpGet("{usuarioId}/pontos-combinados")]
    public async Task<ActionResult<List<PontoCombinadoResponseDto>>> GetPontosCombinadoPorUsuarioIdAsync(Guid usuarioId)
    {
        try
        {
            var pontosCombinados = await _pontoService.GetPontosCombinadoPorUsuarioIdAsync(usuarioId); // Consulta os pontos combinados por usuário
            return Ok(pontosCombinados);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    [HttpGet("{usuarioId}/expediente-hoje")]
    public async Task<ActionResult<bool>> VerificarExpedienteDoDiaAsync(Guid usuarioId)
    {
        try
        {
            var expedienteIniciado = await _pontoService.VerificarExpedienteDoDiaAsync(usuarioId); // Verifica o expediente do dia
            return Ok(expedienteIniciado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }
    #endregion
}