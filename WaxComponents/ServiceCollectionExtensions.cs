using Microsoft.Extensions.DependencyInjection;
using WaxComponents.Services.Theme;

namespace WaxComponents;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWaxComponents(this IServiceCollection services)
    {
        services.AddScoped<IThemeService, ThemeService>();
        
        return services;
    }
}