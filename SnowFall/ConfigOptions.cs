
using Serilog.Configuration;
using SnowFall.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;

namespace SnowFall.Common;

public class ConfigOptions
{
    internal Type? ExceptionFilter { get; set; }

    public void AddExceptionFilter<T>() where T : BaseExceptionFilterAttribute
    {
        ExceptionFilter = typeof(T);
    }

    internal Action<LoggerDestructuringConfiguration>? DestructureAction { get; set; }

    internal Action<SwaggerGenOptions>? SwaggerAction { get; set; }

    internal Action<IList<JsonConverter>>? ConverterAction { get; set; }
}
