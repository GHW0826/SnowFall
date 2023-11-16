
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using System.Text.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using Serilog;
using SnowFall.Filters;
using SnowFall.Helper;
using SnowFall.Common;

namespace SnowFall.Extensions;

public static class HostBuilderExtension
{
    private static IHostBuilder AddSnowFallConfig<T>(this IHostBuilder hostBuilder, ConfigOptions configOptions) where T : BaseConfig
    {
        hostBuilder.ConfigureAppConfiguration((context, config) =>
        {
            T appconfig = context.Configuration.GetSection("AppConfig").Get<T>()
                ?? throw new ArgumentException("Config Argument is Null Check \"AppConfig\" argument in appsettings.json");

            hostBuilder.ConfigureServices((context, services) =>
            {
                foreach (var item in context.Configuration.GetChildren())
                {
                    appconfig[item.Key] = item.Value ?? "";
                }

                services.AddSingleton(appconfig);
                services.AddSingleton<BaseConfig>(appconfig);
                services.AddSingleton(configOptions);
            });
        });

        return hostBuilder;
    }


    /// <summary>
    /// default config add in hostBuilder
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="hostBuilder"></param> 
    /// <param name="configAction"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IHostBuilder AddSnowFallApi<T>(
        this IHostBuilder hostBuilder,
        Action<ConfigOptions, T> configAction
        ) where T : BaseConfig
    {
        // T : BaseConfig
        var configOptions = new ConfigOptions();

        // config setting
        hostBuilder.AddSnowFallConfig<T>(configOptions);

        hostBuilder.ConfigureServices((context, services) =>
        {
            //서비스 컨테이너에서 타입 T의 서비스 인스턴스를 가져옴.
            // 서비스는 T로 지정된 Configuration 클래스
            var config = services.BuildServiceProvider().GetService<T>();


            // 지정된 configOptions와 인자로 받은 configAction 델리게이트를 호출.
            // 설정된 구성 값을 사용해 애플리케이션을 구성하고 설정.
            if ( config != null )
            {
                configAction.Invoke(configOptions, config);
            }
        });

        hostBuilder.AddSnowFall<T>(configOptions);

        // 호스트 빌더를 설정하고, 서비스 컨테이너에 서비스를 추가하는 부분.
        hostBuilder.ConfigureServices((context, services) =>
        {
            // 컨트롤러 서비스를 추가
            services.AddControllers(delegate (MvcOptions options)
            {
                if (configOptions.ExceptionFilter != null)
                {
                    //  예외 필터가 구성 옵션에 포함되어 있으면 이를 컨트롤러의 필터에 추가
                    options.Filters.Add(configOptions.ExceptionFilter);
                }
            }).AddJsonOptions(delegate (JsonOptions options)
            {   // JSON 설정 옵션을 추가

                // JSON 직렬화 시 null 값을 무시하도록 설정
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                // JSON 직렬화에 사용할 컨버터를 추가
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
                // 구성 옵션에 사용자 지정 컨버터 액션이 포함되어 있으면 호출하여 컨버터를 추가
                configOptions.ConverterAction?.Invoke(options.JsonSerializerOptions.Converters);
            });

            //  현재 호스팅 환경이 프로덕션 환경이 아닌지 확인
            if (!context.HostingEnvironment.IsProduction())
            {
                // API 엔드포인트를 문서화하는 데 사용되는 API Explorer 서비스를 추가
                services.AddEndpointsApiExplorer();
                // Swagger를 사용하여 API 문서를 생성하는데 필요한 서비스를 추가
                services.AddSwaggerGen(delegate (SwaggerGenOptions c)
                {
                    // 문서 필터를 추가하여 API 문서에 대문자가 아닌 소문자로 표시되도록 설정
                    c.DocumentFilter<LowercaseDocumentFilter>(Array.Empty<object>());
                    // Swagger에 보안 정의를 추가합니다. 여기서는 Bearer 토큰 인증을 사용하는 경우를 정의
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter token",
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        BearerFormat = "JWT",
                        Scheme = "bearer"
                    });
                    // Swagger에 대한 보안 요구 사항을 추가. 여기서는 Bearer 토큰이 필요하다고 명시
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[0]
                        } });
                    // 구성 옵션에 Swagger 설정 액션이 포함되어 있으면 이를 호출하여 사용자 지정 Swagger 설정을 적용
                    configOptions.SwaggerAction?.Invoke(c);
                });
            }
        });

        return hostBuilder;
    }

    private static IHostBuilder AddSnowFall<T>(this IHostBuilder hostBuilder, ConfigOptions configOptions) where T : BaseConfig
    {
        // Use Serilog
        hostBuilder.AddSnowFallSerilog(configOptions);

        // hostBuilder.AddMarcoApplicationInsights(configOptions);
        // hostBuilder.AddMarcoCors<T>(configOptions);
        hostBuilder.ConfigureServices(delegate (IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddSingleton<HealthChecker>();
        });
        return hostBuilder;
    }


    /// <summary>
    /// Serilog setting
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="hostBuilder"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static IHostBuilder AddSnowFallSerilog(this IHostBuilder hostBuilder, ConfigOptions configOptions)
    {
        hostBuilder.ConfigureAppConfiguration((context, configuation) =>
        {
            configuation.AddJsonFile("serilog.json", optional: true, reloadOnChange: true);
        });

        hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.ApplicationInsights(services.GetService<TelemetryConfiguration>(), TelemetryConverter.Events);

            configOptions.DestructureAction?.Invoke(loggerConfiguration.Destructure);
        });

        return hostBuilder;
    }

}
