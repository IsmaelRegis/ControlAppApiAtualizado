using Microsoft.AspNetCore.Http;

public class RegistrarFimExpedienteRequestDto
{
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string? FotoFimExpediente { get; set; }
    public string? Observacoes { get; set; }
    public IFormFile? FotoFimExpedienteFile { get; set; }
}
