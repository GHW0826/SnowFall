
using Infrastructure.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = "127.0.0.1:6379, defaultDatabase=0";
            options.InstanceName = string.Empty;// "master";
        });

        return services;
    }

    /// <summary>
    /// <see cref="DbContextPoolServiceBuilder{T}" />의 생성을 부트스트랩하는 확장 메서드.
    /// </summary>
    /// <typeparam name="T">DbContext를 확장하는 형식.</typeparam>
    /// <param name="services"><see cref="IServiceCollection" />의 인스턴스.</param>
    /// <returns><see cref="DbContextPoolServiceBuilder{T}" />의 인스턴스.</returns>
    public static DbContextPoolServiceBuilder<T> BeginRegisterDbContextPoolService<T>
        (this IServiceCollection services) where T : DbContext
    {
        return new DbContextPoolServiceBuilder<T>(services);
    }
}
