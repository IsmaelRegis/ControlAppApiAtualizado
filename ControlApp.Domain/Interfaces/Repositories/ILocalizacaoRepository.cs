using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Entities;

namespace ControlApp.Domain.Interfaces.Repositories
{
    public interface ILocalizacaoRepository
    {
        Task<Localizacao?> GetByIdAsync(Guid id); 
        Task<IEnumerable<Localizacao>> ObterLocalizacoesPorTrajetoIdAsync(Guid trajetoId); 
        Task<bool> ExcluirLocalizacoesPorTrajetoIdAsync(Guid trajetoId);
        Task<bool> AdicionarLocalizacaoAsync(Localizacao localizacao);
    }
}
