using ControlApp.Domain.Enums;

public interface IAuditoriaService
{
    Task RegistrarAsync(Guid usuarioId, string nome, string acao, string resumo, UserRole papel);
    Task<IEnumerable<Auditoria>> ObterAsync(Guid? usuarioId, string? acao, string? nome, DateTime? dataInicio, DateTime? dataFim, int page, int pageSize);
}
