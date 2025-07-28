
using ControlApp.Domain.Dtos.Response;
using ControlApp.Domain.Entities;
using ControlApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApp.Domain.Interfaces.Repositories
{
    public interface IPontoRepository
    {
        Task<Ponto> CriarPontoAsync(Ponto ponto);
        Task<Ponto> ObterPontoPorIdAsync(Guid id);
        Task<ICollection<Ponto>> ObterPontosPorTecnicoIdAsync(Guid tecnicoId);
        Task<ICollection<Ponto>> ObterPontosPorPeriodoAsync( DateTime inicio, DateTime fim);
        Task<ICollection<Ponto>> ObterPontosPorUsuarioIdETipo(Guid usuarioId, TipoPonto tipoPonto);
        Task<ICollection<Ponto>> ObterPontosPorTipoAsync(Guid usuarioId, TipoPonto tipoPonto);
        Task<ICollection<Ponto>> ObterPontosPorUsuarioEPeriodoAsync(Guid usuarioId, DateTime inicio, DateTime fim);  
        Task DesativarPontoAsync(Guid id);
        Task AtualizarPontoAsync(Guid id, Ponto pontoAtualizado);
        Task<ICollection<Ponto>> ObterPontoPorUsuarioId(Guid usuarioId);
        Task AtualizarLocalizacoesDoPontoAsync(Guid pontoId, List<Localizacao> localizacoes);
        Task<ICollection<Ponto>> ObterTodosPontosAsync();
        Task<IEnumerable<Ponto>> ObterPontosPorUsuarioNaDataAsync(Guid usuarioId, DateTime data);
    }
}
