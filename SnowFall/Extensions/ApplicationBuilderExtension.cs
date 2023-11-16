
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Swagger;

namespace SnowFall.Extensions;

public static class ApplicationBuilderExtension
{

    public static IApplicationBuilder UseWaterApi(this IApplicationBuilder app, string healthcheck = "/api/healthcheck")
    {
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        if (!environment.IsProduction())
        {
            //ISwaggerProvider가 등록되어 있으면 Swagger Middlerware를 등록한다.
            if (app.ApplicationServices.GetService(typeof(ISwaggerProvider)) != null)
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = string.Empty;
                });

                /*
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "ToDo API",
                        Description = "An ASP.NET Core Web API for managing ToDo items",
                        TermsOfService = new Uri("https://example.com/terms"),
                        Contact = new OpenApiContact
                        {
                            Name = "Example Contact",
                            Url = new Uri("https://example.com/contact")
                        },
                        License = new OpenApiLicense
                        {
                            Name = "Example License",
                            Url = new Uri("https://example.com/license")
                        }
                    });

                    // using System.Reflection;
                    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                });
                */
            }
        }
        app.UseHealthChecks();

        return app;
    }

    private static void UseHealthChecks(this IApplicationBuilder app)
    {
        /*
        //HealthCheckService가 등록되어 있으면 HealthCheck Middlerware를 등록한다.
        if (app.ApplicationServices.GetService(typeof(HealthCheckService)) != null)
        {
            var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            var configOptions = app.ApplicationServices.GetRequiredService<ConfigOptions>();
            string url = string.IsNullOrEmpty(configOptions.HealthCheck) ? "/api/healthcheck" : configOptions.HealthCheck;

            app.UseHealthChecks(url, new HealthCheckOptions
            {
                ResponseWriter = (context, healthReport) =>
                {
                    string path = Path.Combine(environment.ContentRootPath, "health.html");
                    var updown = File.Exists(path) ? File.ReadAllText(path) : "down";

                    return context.Response.WriteAsync(updown);
                }
            });
        }
        */
    }
}
