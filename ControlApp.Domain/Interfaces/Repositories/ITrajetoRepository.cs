using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControlApp.Domain.Entities;

namespace ControlApp.Domain.Interfaces.Repositories
{
    public interface ITrajetoRepository
    {
        Task<Trajeto?> GetByIdAsync(Guid id);
        Task UpdateAsync(Trajeto trajeto);
        Task<IEnumerable<Trajeto>> GetAllAsync();
        Task<IEnumerable<Trajeto>> ObterTrajetosPorUsuarioAsync(Guid usuarioId, DateTime? dataInicio = null, DateTime? dataFim = null);
        Task AddAsync(Trajeto trajeto);
        Task AddTrajetoComLocalizacoesAsync(Trajeto trajeto, List<Localizacao> localizacoes);
        Task<Trajeto?> GetTrajetoDoDiaAsync(Guid usuarioId, DateTime date);
        void DetachEntity<T>(T entity) where T : class;
    }
}
