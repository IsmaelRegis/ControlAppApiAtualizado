using System;

namespace ControlApp.Domain.Dtos.Response
{
    public class RegistrarFimExpedienteResponseDto
    {
        public TimeSpan HorasTrabalhadas { get; set; }
        public TimeSpan HorasExtras { get; set; }
        public TimeSpan HorasDevidas { get; set; }
        public string? DistanciaTotal { get; set; }    
        public string? FotoFimExpediente { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }
}
