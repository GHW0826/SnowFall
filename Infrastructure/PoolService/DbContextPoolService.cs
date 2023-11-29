
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Emit;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Helper;

public class DbContextPoolService<T> where T : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _typeMappings = new ();

    internal DbContextPoolService(IServiceCollection services,
           Dictionary<string, Action<IServiceProvider, DbContextOptionsBuilder>> connectionDetails)
    {
        IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var ContextPoolSize = int.Parse(configuration["ContextPoolSize"] ?? throw new Exception ("ContextPoolSize is null in appsetting.json"));

        // 연결 정보를 포함하는 딕셔너리 connectionDetails
        // d는 각 연결 정보
        foreach (var d in connectionDetails)
        {
            // 각 연결 정보에 대한 DbContext 유형을 동적으로 생성.
            var contextType = BuildContextType(d.Key);

            // 연결 정보에 대한 옵션을 설정하는 역할. (람다)
            // AddDbContextPool 메소드에 전달하여 DbContext 옵션을 구성
            void OptionsAction(IServiceProvider provider, DbContextOptionsBuilder builder)
            {
                d.Value(provider, builder);
            }

            // Refliection을 사용해 EntityFrameworkServiceCollectionExtensions.AddDbContextPool을 호출.
            // 이 메소드는 DbContext 풀을 서비스 컬렉션에 추가하고 옵션 설정을 수행.
            typeof(EntityFrameworkServiceCollectionExtensions).GetMethods()
                .First(
                    m => m.Name == "AddDbContextPool" &&    // 메소드 이름이 AddDbContextPool이고
                    m.GetGenericArguments().Length == 1 &&  // 제네릭 매개변수가 하나 있고
                    m.GetParameters()                       // 2개의 형식 인자를 가진 메소드를 찾음
                        .First(p => p.Name == "optionsAction")
                        .ParameterType.GenericTypeArguments.Length == 2)
                .MakeGenericMethod(contextType)             // 찾은 메소드를 contextType 제네릭으로 만듦
                .Invoke(                                    // 동적으로 만든 메소드를 호출. 
                    null,
                    new object[] {services, (Action<IServiceProvider, DbContextOptionsBuilder>) OptionsAction,
                    ContextPoolSize
                    });

            // _typeMappings 딕셔너리에 각 연결 정보에 해당하는 DbContext 유형을 추가.
            _typeMappings.Add(d.Key, contextType);
        } 
        _serviceProvider = services.BuildServiceProvider();

        // 결과적으로 여러 DbContext 풀을 설정하고 풀의 크기를 1200으로 고정함
        // 1200 고정에서 appsetting.json 값으로 수정
    }

    /// <summary>
    /// Pool에 등록된 Dbcontext 객체 반환.
    /// 서비스 등록시 함께 등록한 이름의 객체를 찾아 반환
    /// GetService로 DbContext 객체를 꺼내도 DI된 DbContext를 꺼냄
    /// </summary>
    /// <param name="name"> 등록시 설정한 Dbcontext의 Key 이름 </param>
    /// <returns> DbContext 객체 반환 <see cref="DbContext" /></returns>
    public T? GetDbContext(string name) => (T?)_serviceProvider.GetService(_typeMappings[name]);

    /// <summary>
    /// 각 등록된 연결 세트에 대해 <see cref="DbContext" />의 하나의 인스턴스를 검색.
    /// </summary>
    /// <returns>등록된 이름과 DbContext를 포함하는 Dictionary</returns>
    public Dictionary<string, T?> GetAllContexts() 
        => _typeMappings.Select(m => new KeyValuePair<string, T?>(m.Key, (T?)_serviceProvider.GetService(m.Value))) as Dictionary<string, T?>
            ?? throw new Exception();


    /// <summary>
    /// 등록된 커넥션 설정의 이름들 반환
    /// </summary>
    /// <returns> 등록된 key strings <see cref="IEnumerable{T}." /></returns>
    public IEnumerable<string> GetContextNames() => _typeMappings.Keys;


    private Type BuildContextType(string name)
    {
        // 동적 어셈블리, 모듈, 타입을 생성.
        // 새로운 어셈블리, 모듈이 생성되고, 그 안에 새로운 DbContext 파생 클래스가 정의됨.
        var typeBuilder = AssemblyBuilder
                            .DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run)
                            .DefineDynamicModule("core").DefineType($"DbContext_{GenerateSlug(name)}");

        // 원본 DbContext 클래스의 유형(Type)을 가져옴.
        var parentType = typeof(T);
        // 원본 DbContext 클래스의 생성자 정보를 가져오는데 사용됨.
        // 가져온 생성자는 DbContextOptions을 매개변수로 받음.
        var parentConstructor = parentType.GetConstructor(new[] { typeof(DbContextOptions) })
                ?? throw new Exception("Context pool parentContructor is null");

        // 동적으로 생성된 파생 클래스의 부모 클래스를 설정
        // 파생 클래스는 원본인 DbContext를 상속하게 됨.
        typeBuilder.SetParent(parentType);

        // 파생 클래스의 생성자를 정의. 
        // 생성자는 DbContextOptions을 매개변수로 받아 부모 클래스의 생성자를 호출하는 역할을 함.
        var constructorBuilder = typeBuilder.DefineConstructor(
            MethodAttributes.Public, 
            CallingConventions.Standard,
            new[] { typeof(DbContextOptions) }
           );

        // 생성자의 IL (intermediate Language) 코드를 생성하는데 사용됨.
        // Ldarg_0, Ldarg_1은 생성자의 매개변수를 스택에 로드하고, 
        // Call 명령을 사용해 부모 생성자를 호출.
        var constructorIlGenerator = constructorBuilder.GetILGenerator();
        constructorIlGenerator.Emit(OpCodes.Ldarg_0);
        constructorIlGenerator.Emit(OpCodes.Ldarg_1);
        constructorIlGenerator.Emit(OpCodes.Call, parentConstructor);
        // 생성자 코드를 마무리하고 반환.
        constructorIlGenerator.Emit(OpCodes.Ret);

        // 결과적으로 DbContext를 상속하는 동적으로 생성된 파생 클래스를 만듦.
        // 이 동적 클래스는 원본 DbContext와 유사한 동작을 수행하고,
        // DbContextOptions를 허용하는 생성자를 가짐.

        /* 아래 내용과 비슷하게 동작하는 듯.
        
            public class MyDynamicDbContext<T> : DbContext where T : DbContext
            {
                public MyDynamicDbContext(DbContextOptions options) : base(options)
                {}
            }
        */

        return typeBuilder.CreateType();
    }

    private string GenerateSlug(string phrase)
    {
        string str = phrase.ToLower();
        // invalid chars           
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        // convert multiple spaces into one space   
        str = Regex.Replace(str, @"\s+", " ").Trim();
        // cut and trim 
        str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
        str = Regex.Replace(str, @"\s", "-"); // hyphens   
        return str;
    }
}
