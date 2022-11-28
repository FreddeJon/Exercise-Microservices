using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Common.Settings;

namespace Play.Common.MongoDB;

public static class Extensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

        services.AddSingleton(serviceProvider =>
        {
            var configuration = serviceProvider.GetService<IConfiguration>()
            ?? throw new ArgumentException(nameof(IConfiguration));
            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
            var mongoClient = new MongoClient(mongoDbSettings?.ConnectionString ?? throw new ArgumentException(nameof(mongoDbSettings)));

            return mongoClient.GetDatabase(serviceSettings?.ServiceName ?? throw new ArgumentException(nameof(serviceSettings)));
        });



        return services;
    }

    public static IServiceCollection AddMongoRepository<TEntity>(this IServiceCollection services, string collectionName) where TEntity : IEntity
    {
        services.AddScoped<IRepository<TEntity>>(serviceProvider =>
        {
            var database = serviceProvider.GetService<IMongoDatabase>() ?? throw new ArgumentException(nameof(IMongoDatabase));
            return new MongoRepository<TEntity>(database, collectionName);
        });

        return services;
    }
}
