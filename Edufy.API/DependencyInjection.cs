using System.Text.Json.Serialization;
using Edufy.API.Common.Conventions;
using Edufy.API.Middlewares;
using Edufy.API.Common.Extensions;

namespace Edufy.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services
            .AddControllers(options =>
            {
                options.Conventions.Add(new ApiConvention());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services
            .AddEndpointsApiExplorer()
            .AddSwaggerConfiguration()
            .AddProblemDetails()
            .AddValidationConfiguration()
            .AddCorsConfiguration()
            .AddHttpContextAccessor();

        return services;
    }

    public static WebApplication UsePresentation(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ExceptionMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Edufy API v1");
            options.RoutePrefix = string.Empty;
        });

        app.UseCors("CorsPolicy");
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
