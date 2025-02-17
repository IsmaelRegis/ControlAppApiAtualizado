using Microsoft.AspNetCore.Http;

public class RegistrarInicioExpedienteRequestDto
{
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string? FotoInicioExpediente { get; set; }
    public IFormFile? FotoInicioExpedienteFile { get; set; }
    public string? Observacoes { get; set; }
}
