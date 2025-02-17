using System;

namespace ControlApp.Domain.Dtos.Response
{
    public class TrajetoResponseDto
    {
        public Guid Id { get; set; }  
        public Guid PontoInicioId { get; set; } 

        public string? LatitudeInicio { get; set; }
        public string? LongitudeInicio { get; set; }
        public string? LatitudeFim { get; set; }
        public string? LongitudeFim { get; set; }

        public override string ToString()
        {
            return $"Trajeto (ID: {Id}): Inicio({LatitudeInicio}, {LongitudeInicio}) -> Fim({LatitudeFim}, {LongitudeFim})";
        }
    }
}
