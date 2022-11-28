using System.Linq.Expressions;
using MongoDB.Driver;

namespace Play.Common.MongoDB;

public class MongoRepository<TEntity> : IRepository<TEntity> where TEntity : IEntity
{
    private readonly IMongoCollection<TEntity> _dbCollection;
    private readonly FilterDefinitionBuilder<TEntity> filterBuilder = Builders<TEntity>.Filter;

    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        _dbCollection = database.GetCollection<TEntity>(collectionName);
    }

    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync()
    {
        return await _dbCollection.Find(filterBuilder.Empty).ToListAsync();
    }
    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await _dbCollection.Find(filter).ToListAsync();

    }

    public async Task<TEntity> GetAsync(Guid id)
    {
        var filter = filterBuilder.Eq(entity => entity.Id, id);
        return await _dbCollection.Find(filter).FirstOrDefaultAsync();
    }
    public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await _dbCollection.Find(filter).FirstOrDefaultAsync();
    }
    public async Task CreateAsync(TEntity entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        await _dbCollection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(TEntity entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var filter = filterBuilder.Eq(existingEntyity => existingEntyity.Id, entity.Id);

        await _dbCollection.ReplaceOneAsync(filter, entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var filter = filterBuilder.Eq(entity => entity.Id, id);
        await _dbCollection.DeleteOneAsync(filter);
    }
}