public class RegistrarInicioExpedienteResponseDto
{
    public Guid PontoId { get; set; }                
    public DateTime InicioExpediente { get; set; }
    public string? Latitude { get; set; }      
    public string? Longitude { get; set; }     
    public string? FotoInicioExpediente { get; set; }
    public string? Observacoes { get; set; }

}
