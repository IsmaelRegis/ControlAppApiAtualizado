using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Dtos.Request
{
    public class PaginacaoRequestDto
    {
        public int Pagina { get; set; }
        public int TamanhoPagina { get; set; }
    }
}
