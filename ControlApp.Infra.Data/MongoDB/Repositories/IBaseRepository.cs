/*using System.Linq.Expressions;

namespace ControlApp.Infra.Data.MongoDB.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<TEntity> CriarAsync(TEntity entidade);
        Task<TEntity> ObterPorIdAsync(Guid id);
        Task<IEnumerable<TEntity>> ObterTodosAsync();
        Task<IEnumerable<TEntity>> ObterPorFiltroAsync(Expression<Func<TEntity, bool>> filtro);
        Task AtualizarAsync(Guid id, TEntity entidade);
        Task DeletarAsync(Guid id);
    }
}*/
