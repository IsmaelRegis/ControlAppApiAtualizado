/*using System.Linq.Expressions;
using ControlApp.Infra.Data.MongoDB.Configurations;
using MongoDB.Driver;

namespace ControlApp.Infra.Data.MongoDB.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity>
           where TEntity : class
    {
        protected readonly IMongoCollection<TEntity> _collection;

        public BaseRepository(MongoDbContext context, string collectionName)
        {
            _collection = context.GetCollection<TEntity>(collectionName);
        }

        public virtual async Task<TEntity> CriarAsync(TEntity entidade)
        {
            await _collection.InsertOneAsync(entidade);
            return entidade;
        }

        public virtual async Task<TEntity> ObterPorIdAsync(Guid id)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> ObterTodosAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public virtual async Task AtualizarAsync(Guid id, TEntity entidade)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", id);
            await _collection.ReplaceOneAsync(filter, entidade);
        }

        public virtual async Task DeletarAsync(Guid id)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", id);
            await _collection.DeleteOneAsync(filter);
        }


        public virtual async Task<IEnumerable<TEntity>> ObterPorFiltroAsync(
            Expression<Func<TEntity, bool>> filtro)
        {
            return await _collection.Find(filtro).ToListAsync();
        }
    }
}
*/