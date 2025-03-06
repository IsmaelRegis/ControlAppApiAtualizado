using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Dtos.Response
{
    public class EmpresaResponseDto
    {
        public Guid EmpresaId { get; set; }
        public bool Ativo { get; set; }
        public string? NomeDaEmpresa { get; set; }
        public EnderecoDto? Endereco { get; set; }
        public List<TecnicoResponseDto>? Tecnicos { get; set; }
    }
}
