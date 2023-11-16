
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Helper;

public class DbContextPoolServiceBuilder<T> where T : DbContext
{
    private readonly Dictionary<string, Action<IServiceProvider, DbContextOptionsBuilder>>
        _connectionDetails = new Dictionary<string, Action<IServiceProvider, DbContextOptionsBuilder>>();

    private readonly IServiceCollection _serviceCollection;

    internal DbContextPoolServiceBuilder(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    /// <summary>
    /// 서비스 빌더에 연결 세부 정보 세트를 추가. 이 메서드의 각 호출은 <see cref="DbContext" />의 인스턴스를 등록하는 것과 동일.
    /// </summary>
    /// <param name="contextName"><see cref="DbContext" />를 검색하는 데 사용될 이름.</param>
    /// <param name="optionsAction">
    /// <see cref="DbContext" /> 인스턴스를 구성하는 데 사용되는 대리자.
    /// 이는 서비스 공급자에 수동으로 <see cref="DbContext" />를 등록하는 것과 동일.
    /// </param>
    /// <returns><see cref="DbContextPoolServiceBuilder{T}" />의 현재 인스턴스.</returns>
    internal DbContextPoolServiceBuilder<T> AddConnectionDetails (
        string contextName,
        Action<IServiceProvider, DbContextOptionsBuilder> optionsAction
    )
    {
        _connectionDetails.Add(contextName, optionsAction);
        return this;
    }

    /// <summary>
    /// 더 이상 연결 세부 정보가 추가되지 않을 것을 나타내어, 멀티플렉서를 빌드하고 서비스 컨테이너에 등록.
    /// </summary>
    /// <returns><see cref="IServiceCollection" />의 인스턴스.</returns>
    internal IServiceCollection FinishRegisterDbContextPoolService()
    {
        var dbContextPoolService = new DbContextPoolService<T>(_serviceCollection, _connectionDetails);
        return _serviceCollection.AddSingleton(dbContextPoolService);
    }
}
