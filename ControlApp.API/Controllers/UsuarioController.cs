using ControlApp.Domain.Dtos.Request;
using ControlApp.Domain.Dtos.Response;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Interfaces.Security;
using ControlApp.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ITokenSecurity _tokenSecurity;

    public UsuarioController(IUsuarioService usuarioService, ITokenSecurity tokenSecurity)
    {
        _usuarioService = usuarioService;
        _tokenSecurity = tokenSecurity;
    }

    [HttpPost("authenticate")]
    [AllowAnonymous]
    public async Task<ActionResult> Authenticate([FromBody] AutenticarUsuarioRequestDto request)
    {
        try
        {
            var usuarioResponse = await _usuarioService.AuthenticateUsuarioAsync(request);

            if (usuarioResponse == null)
            {
                return Unauthorized("CPF ou senha inválidos.");
            }

            var token = _tokenSecurity.CreateToken(usuarioResponse.UsuarioId, usuarioResponse.Role);

            return Ok(new
            {
                Token = token,
                Usuario = usuarioResponse
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromForm] CriarUsuarioRequestDto request)
    {
        try
        {
            var response = await _usuarioService.CreateUsuarioAsync(request);

            var usuarioResponse = new CriarUsuarioResponseDto
            {
                UsuarioId = response.UsuarioId,
                Nome = response.Nome,
                UserName = response.UserName,
                Email = response.Email,
                Role = response.Role,
                Ativo = response.Ativo,
                FotoUrl = response.FotoUrl,
                IsOnline = response.IsOnline,
                HoraEntrada = response.HoraEntrada,
                HoraSaida = response.HoraSaida,
                HoraAlmocoInicio = response.HoraAlmocoInicio,
                HoraAlmocoFim = response.HoraAlmocoFim,
            };

            return Ok(usuarioResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpPut("update/{id}")]
    public async Task<ActionResult> Update([FromRoute] Guid id, [FromForm] AtualizarUsuarioRequestDto request)
    {
        try
        {
            var response = await _usuarioService.UpdateUsuarioAsync(id, request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPut("update-location/{usuarioId}")]
    public async Task<ActionResult> AtualizarLocalizacaoAtual(
    [FromRoute] Guid usuarioId,
    [FromQuery] string latitude,
    [FromQuery] string longitude)
    {
        try
        {
            await _usuarioService.AtualizarLocalizacaoAtualAsync(usuarioId, latitude, longitude);
            return Ok("Localização atualizada com sucesso.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }


    [Authorize(Roles = "Administrador")]
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            await _usuarioService.DeleteAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("tecnicos")]
    public async Task<IActionResult> GetTecnicos()
    {
        var tecnicos = await _usuarioService.GetAllTecnicosAsync();
        return Ok(tecnicos);
    }

    [HttpGet("{usuarioId}/status")]
    public async Task<IActionResult> GetUsuarioStatus(Guid usuarioId)
    {
        var usuario = await _usuarioService.GetByIdAsync(usuarioId);
        if (usuario == null)
            return NotFound("Usuário não encontrado.");

        return Ok(usuario.Ativo);
    }

    [HttpGet("{usuarioId}")]
    public async Task<IActionResult> GetUsuarioById(Guid usuarioId)
    {
        var usuario = await _usuarioService.GetByIdAsync(usuarioId);
        if (usuario == null)
            return NotFound("Usuário não encontrado.");
        return Ok(usuario);
    }
}