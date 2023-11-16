
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace SnowFall.Extensions;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// Add Default Config for Api Service Collection
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSnowFallApi(this IServiceCollection services)
    {
        // Use MediatR
        // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddEndpointsApiExplorer();

        // Swagger
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
        });

/*
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "my-site",
                ValidAudience = "my-site",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("holywater"))
            };
        });
*/

        return services;
    }
}
