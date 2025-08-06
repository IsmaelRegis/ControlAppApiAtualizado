public class RegistrarInicioPausaResponseDto
{
    public Guid PontoId { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataHoraInicioPausa { get; set; }
    public TimeSpan HorasTrabalhadas { get; set; }

}
