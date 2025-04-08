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
        Task<Localizacao?> GetByIdAsync(Guid id); // Útil para buscar uma localização específica por ID
        Task<IEnumerable<Localizacao>> ObterLocalizacoesPorTrajetoIdAsync(Guid trajetoId); // Busca por TrajetoId
        Task<bool> AdicionarLocalizacaoAsync(Localizacao localizacao);
    }
}
