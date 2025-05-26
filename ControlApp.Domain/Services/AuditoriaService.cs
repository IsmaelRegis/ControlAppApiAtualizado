using System.Reflection.Emit;
using ControlApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

public class AuditoriaService : IAuditoriaService
{
    private readonly IAuditoriaRepository _auditoriaRepository;

    public AuditoriaService(IAuditoriaRepository auditoriaRepository)
    {
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task RegistrarAsync(Guid usuarioId, string nome, string acao, string resumo, UserRole papel)
    {
        var auditoria = new Auditoria
        {
            UsuarioId = usuarioId,
            NomeUsuario = nome,
            Acao = acao,
            Resumo = resumo,
            Papel = papel,
            DataHora = DateTime.Now
        };

        await _auditoriaRepository.RegistrarAsync(auditoria);
    }

    public async Task<IEnumerable<Auditoria>> ObterAsync(Guid? usuarioId, string? acao, string? nome, DateTime? dataInicio, DateTime? dataFim, int page, int pageSize)
    {
        var query = _auditoriaRepository.Query();

        if (usuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == usuarioId.Value);

        if (!string.IsNullOrWhiteSpace(acao))
            query = query.Where(x => x.Acao.Contains(acao));

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(x => x.NomeUsuario.Contains(nome));

        if (dataInicio.HasValue)
            query = query.Where(x => x.DataHora >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(x => x.DataHora <= dataFim.Value);

        return await query
            .OrderByDescending(x => x.DataHora)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
