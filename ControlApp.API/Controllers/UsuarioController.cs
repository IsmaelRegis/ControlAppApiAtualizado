using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    private readonly ITokenManager _tokenManager;

    #region Construtor
    public UsuarioController(IUsuarioService usuarioService, ITokenSecurity tokenSecurity, ITokenManager tokenManager)
    {
        _usuarioService = usuarioService; // Injeção de dependência do serviço de usuário
        _tokenSecurity = tokenSecurity;   // Injeção de dependência do serviço de token
        _tokenManager = tokenManager;
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
            // Capturar o User-Agent do cabeçalho da requisição
            string deviceInfo = Request.Headers["User-Agent"].ToString();

            // Determinar a audience automaticamente
            string audience = "VibeService"; // Valor padrão
            var usuarioResponse = await _usuarioService.AuthenticateUsuarioAsync(request, deviceInfo, audience);
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

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        try
        {
            await _usuarioService.LogoutUsuarioAsync(request.UsuarioId, request.Token);
            return Ok(new { Mensagem = "Logout realizado com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Mensagem = "Erro ao realizar logout", Detalhes = ex.Message });
        }
    }


    [Authorize(Roles = "SuperAdministrador")]
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

    [Authorize]
    [HttpPut("update/{id}")]
    public async Task<ActionResult> Update([FromRoute] Guid id, [FromForm] AtualizarUsuarioRequestDto request)
    {
        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        if (role == "Visitante")
            return Forbid("Visitantes não têm permissão para atualizar.");

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

    [Authorize(Roles = "SuperAdministrador")]
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
    public async Task<IActionResult> GetTecnicos([FromQuery] PaginacaoRequestDto paginacao = null)
    {
        // Se nenhum parâmetro de paginação for fornecido, o método do serviço usará os valores padrão
        var tecnicosPaginados = await _usuarioService.GetTecnicosPaginadosAsync(paginacao);
        return Ok(tecnicosPaginados);
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

    [HttpGet("tecnicos/online")]
    public async Task<IActionResult> GetTecnicosOnline()
    {
        try
        {
            var tecnicosOnline = await _usuarioService.GetTecnicosOnlineAsync();
            return Ok(tecnicosOnline);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao obter técnicos online: {ex.Message}");
        }
    }


    [Authorize(Roles = "SuperAdministrador")]
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

    [Authorize]
    [HttpPut("empresa/{empresaId}")]
    public async Task<ActionResult> UpdateEmpresa(Guid empresaId, [FromBody] AtualizarEmpresaRequestDto request)
    {
        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        if (role == "Visitante")
            return Forbid("Visitantes não têm permissão para atualizar.");

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

    [Authorize(Roles = "SuperAdministrador")]
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

    // No ControlApp - UsuarioController.cs
    [HttpPost("validate-token")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequestDto request)
    {
        if (request == null || string.IsNullOrEmpty(request.Token) || request.UserId == Guid.Empty)
            return BadRequest();

        // Extrai o jti do token
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(request.Token);
        var tokenId = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;

        if (string.IsNullOrEmpty(tokenId))
            return StatusCode(440);

        // Valida apenas o tokenId
        var isValid = await _tokenManager.ValidateTokenAsync(tokenId, request.UserId);

        if (!isValid)
            return StatusCode(440, new { Error = "MultipleLoginConflict", Message = "Token não está mais ativo." });

        return Ok(true);
    }
}