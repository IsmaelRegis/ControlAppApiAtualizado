using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Entities;

namespace ControlApp.Domain.Interfaces.Repositories
{
    public interface IEmpresaRepository
    {
        Task<Empresa> CriarEmpresaAsync(Empresa empresa);
        Task<Empresa?> ObterEmpresaPorIdAsync(Guid id);
        Task<IEnumerable<Empresa>> GetAllEmpresasAsync();
        Task AtualizarEmpresaAsync(Empresa empresa);
        Task ExcluirEmpresaAsync(Guid id);
    }
}
