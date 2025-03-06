using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Dtos.Request
{
    public class EnderecoRequestDto
    {
        public string? Cep { get; set; } // Opcional
        public string? Numero { get; set; } // Opcional
        public string? Complemento { get; set; } // Opcional
    }
}
