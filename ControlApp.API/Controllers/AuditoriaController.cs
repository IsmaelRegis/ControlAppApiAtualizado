using ControlApp.Domain.Entities;
using ControlApp.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControlApp.API.Controllers
{
    [ApiController]
    [Route("controlapp/[controller]")]
    public class AuditoriaController : ControllerBase
    {
        private readonly IAuditoriaService _auditoriaService;

        public AuditoriaController(IAuditoriaService auditoriaService)
        {
            _auditoriaService = auditoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] Guid? usuarioId,
            [FromQuery] string? nome,
            [FromQuery] string? acao, // pode ser Login, Logout, ou null pra ambos
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var resultado = await _auditoriaService.ObterAsync(
                    usuarioId, acao, nome, dataInicio, dataFim, page, pageSize
                );

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao buscar registros de auditoria", Detalhes = ex.Message });
            }
        }
    }
}
