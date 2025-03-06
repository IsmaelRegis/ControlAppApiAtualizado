using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Entities
{
    public class Empresa
    {
        public Guid EmpresaId { get; set; }
        public bool Ativo { get; set; } = true;
        public string? NomeDaEmpresa { get; set; }
        public Endereco? Endereco { get; set; }
        public ICollection<Tecnico> Tecnicos { get; set; } = new List<Tecnico>();
    }
}
