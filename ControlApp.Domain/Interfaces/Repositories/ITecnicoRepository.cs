using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Dtos;
using ControlApp.Domain.Entities;

namespace ControlApp.Domain.Interfaces.Repositories
{
    public interface ITecnicoRepository : IUsuarioRepository
    {
        Task<ICollection<Ponto>> ObterPontosPorTecnicoAsync(Guid tecnicoId);
        Task<ICollection<Ponto>> ObterPontosPorPeriodoAsync(Guid tecnicoId, DateTime inicio, DateTime fim);
    }
}
