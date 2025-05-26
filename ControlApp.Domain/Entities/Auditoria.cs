using ControlApp.Domain.Enums;

public class Auditoria
{
    public Guid AuditoriaId { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public string? NomeUsuario { get; set; } = string.Empty;
    public string? Acao { get; set; } = string.Empty;
    public string? Resumo { get; set; } = string.Empty;
    public DateTime? DataHora { get; set; } = DateTime.Now;
    public UserRole Papel { get; set; }
}
