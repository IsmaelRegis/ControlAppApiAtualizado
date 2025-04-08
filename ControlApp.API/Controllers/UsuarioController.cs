using ControlApp.Domain.Dtos.Request;
using ControlApp.Domain.Dtos.Response;
using ControlApp.Domain.Interfaces.Security;
using ControlApp.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("controlapp/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ITokenSecurity _tokenSecurity;

    #region Construtor
    public UsuarioController(IUsuarioService usuarioService, ITokenSecurity tokenSecurity)
    {
        _usuarioService = usuarioService; // Injeção de dependência do serviço de usuário
        _tokenSecurity = tokenSecurity;   // Injeção de dependência do serviço de token
    }
    #endregion

    #region Endpoints
    /* 
     * Endpoints para gerenciamento de usuários e empresas.
     */

    [HttpPost("authenticate")]
    [AllowAnonymous]
    public async Task<ActionResult> Authenticate([FromBody] AutenticarUsuarioRequestDto request)
    {
        try
        {
            var usuarioResponse = await _usuarioService.AuthenticateUsuarioAsync(request);
            return Ok(usuarioResponse); // Retorna diretamente o usuarioResponse, que já contém o token
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Usuário inválido, username ou senha inválidos."); 
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Erro interno no servidor", Detail = ex.Message }); // Retorna 500 para outros erros
        }
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromForm] CriarUsuarioRequestDto request)
    {
        try
        {
            var response = await _usuarioService.CreateUsuarioAsync(request); // Cria novo usuário
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
            var response = await _usuarioService.UpdateUsuarioAsync(id, request); // Atualiza usuário
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
            await _usuarioService.AtualizarLocalizacaoAtualAsync(usuarioId, latitude, longitude); // Atualiza localização
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

    [HttpPost("registrar-localizacao/{usuarioId}")]
    public async Task<ActionResult> RegistrarLocalizacao(
    [FromRoute] Guid usuarioId,
    [FromQuery] string latitude,
    [FromQuery] string longitude)
    {
        try
        {
            var resultado = await _usuarioService.AdicionarRegistroLocalizacaoAsync(usuarioId, latitude, longitude);
            if (resultado)
                return Ok("Localização registrada com sucesso.");
            else
                return BadRequest("Não foi possível registrar a localização.");
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
            await _usuarioService.DeleteAsync(id); // Exclui usuário
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
        var tecnicos = await _usuarioService.GetAllTecnicosAsync(); // Lista todos os técnicos
        return Ok(tecnicos);
    }

    [HttpGet("{usuarioId}/status")]
    public async Task<IActionResult> GetUsuarioStatus(Guid usuarioId)
    {
        var usuario = await _usuarioService.GetByIdAsync(usuarioId); // Obtém status do usuário
        if (usuario == null)
            return NotFound("Usuário não encontrado.");
        return Ok(usuario.Ativo);
    }

    [HttpGet("{usuarioId}")]
    public async Task<IActionResult> GetUsuarioById(Guid usuarioId)
    {
        var usuario = await _usuarioService.GetByIdAsync(usuarioId); // Obtém usuário por ID
        if (usuario == null)
            return NotFound("Usuário não encontrado.");
        return Ok(usuario);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost("empresa")]
    public async Task<ActionResult> CreateEmpresa([FromBody] CriarEmpresaRequestDto request)
    {
        try
        {
            var response = await _usuarioService.CreateEmpresaAsync(request); // Cria nova empresa
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("empresa/{empresaId}")]
    public async Task<IActionResult> GetEmpresaById(Guid empresaId)
    {
        var empresa = await _usuarioService.GetEmpresaByIdAsync(empresaId); // Obtém empresa por ID
        if (empresa == null)
            return NotFound("Empresa não encontrada.");
        return Ok(empresa);
    }

    [HttpGet("empresa")]
    public async Task<IActionResult> GetAllEmpresas()
    {
        var empresas = await _usuarioService.GetAllEmpresasAsync(); // Lista todas as empresas
        return Ok(empresas);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("empresa/{empresaId}")]
    public async Task<ActionResult> UpdateEmpresa(Guid empresaId, [FromBody] AtualizarEmpresaRequestDto request)
    {
        try
        {
            var response = await _usuarioService.UpdateEmpresaAsync(empresaId, request); // Atualiza empresa
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("empresa/{empresaId}")]
    public async Task<ActionResult> DeleteEmpresa(Guid empresaId)
    {
        try
        {
            await _usuarioService.DeleteEmpresaAsync(empresaId); // Exclui empresa
            return Ok("Empresa excluída com sucesso.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    #endregion
}