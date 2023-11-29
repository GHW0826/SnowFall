
using Infrastructure.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedLockNet.SERedis.Configuration;
using RedLockNet.SERedis;
using StackExchange.Redis;
using System.Configuration;
using Infrastructure.Redis;

namespace Infrastructure;

public static class DependencyInjection
{

    public static IServiceCollection AddSnowFallRedis(this IServiceCollection services, IConfiguration configuration)
    {
        // NRedisStack
        services.AddScoped(cfg =>
        {
            IConnectionMultiplexer multiplexer = 
                ConnectionMultiplexer.Connect(configuration["RedisConnectionString"] ?? throw new Exception("Redis connection string is null"));
            return multiplexer.GetDatabase();
        });

        // Redis의 분산 잠금(Distributed Lock)
        services.AddScoped(cfg =>
        {
            return RedLockFactory.Create(new List<RedLockMultiplexer>{
                ConnectionMultiplexer.Connect(configuration["RedisConnectionString"] ?? throw new Exception("Redis connection string is null"))
            });
        });

        // pub/sub
        //중복 수신을 피하기 위해 Singleton
        services.AddSingleton(cfg =>
        {
            IConnectionMultiplexer multiplexer =
                ConnectionMultiplexer.Connect(configuration["RedisConnectionString"] ?? throw new Exception("Redis connection string is null"));
            return multiplexer.GetSubscriber();
        });
        services.AddSingleton<RedisSubscribeService>();

        services.AddScoped<RedisPublishService>();
        return services;
    }

    public static IServiceCollection AddSnowFallContextPool<T>(
        this IServiceCollection services,
        string contextName,
        string connectionString
        ) where T : DbContext
    {
        var shardInfoContextPoolMS = services.BeginRegisterDbContextPoolService<T>();
        shardInfoContextPoolMS.AddConnectionDetails(contextName, (provider, builder) => builder.UseMySQL(connectionString));
        shardInfoContextPoolMS.FinishRegisterDbContextPoolService();

        return services;
    }
    public static IServiceCollection AddSnowFallContextPool<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        string contextName,
        string connectionStringKey,
        int ShardCnt) where T : DbContext
    {
        var shardInfoContextPoolMS = services.BeginRegisterDbContextPoolService<T>();
        for (int i = 0; i < ShardCnt; ++i)
        {
            var shardContextName = contextName + i.ToString();
            var shardConnectionString = configuration?.GetConnectionString(connectionStringKey) + i.ToString()
                ?? throw new Exception($"{connectionStringKey} is null in config");
            shardInfoContextPoolMS.AddConnectionDetails(shardContextName, (provider, builder) => builder.UseMySQL(shardConnectionString));
        }
        shardInfoContextPoolMS.FinishRegisterDbContextPoolService();

        return services;
    }

    /// <summary>
    /// <see cref="DbContextPoolServiceBuilder{T}" />의 생성을 부트스트랩하는 확장 메서드.
    /// </summary>
    /// <typeparam name="T">DbContext를 확장하는 형식.</typeparam>
    /// <param name="services"><see cref="IServiceCollection" />의 인스턴스.</param>
    /// <returns><see cref="DbContextPoolServiceBuilder{T}" />의 인스턴스.</returns>
    internal static DbContextPoolServiceBuilder<T> BeginRegisterDbContextPoolService<T>
        (this IServiceCollection services) where T : DbContext
    {
        return new DbContextPoolServiceBuilder<T>(services);
    }
}
