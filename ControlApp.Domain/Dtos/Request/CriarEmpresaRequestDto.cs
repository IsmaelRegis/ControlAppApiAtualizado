using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Dtos.Response;

namespace ControlApp.Domain.Dtos.Request
{
    public class CriarEmpresaRequestDto
    {
        public string? NomeDaEmpresa { get; set; }
        public EnderecoRequestDto? Endereco { get; set; } // Opcional
    }
}
