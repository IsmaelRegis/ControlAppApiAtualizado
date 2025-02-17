using ControlApp.Domain.Entities;

public class Trajeto
{
    public Guid Id { get; set; }
    public DateTime Data { get; set; }
    public Guid UsuarioId { get; set; }
    public Tecnico? Tecnico { get; set; }
    public List<Localizacao> Localizacoes { get; set; } = new List<Localizacao>();
    public double DistanciaTotalKm { get; set; }
    public TimeSpan DuracaoTotal { get; set; }
    public string Status { get; set; } = "Em andamento";
}
