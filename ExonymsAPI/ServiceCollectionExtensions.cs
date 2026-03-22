using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ExonymsAPI.Service;
using ExonymsAPI.Service.Gatherers;
using ExonymsAPI.Service.Processors;
using ExonymsAPI.Client.TransliterationAPI;
using ExonymsAPI.Configuration;
using NuciAPI.Client;

namespace ExonymsAPI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigurations(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            TransliterationSettings transliterationSettings = new();
            SecuritySettings securitySettings = new();

            configuration.Bind(nameof(TransliterationSettings), transliterationSettings);
            configuration.Bind(nameof(SecuritySettings), securitySettings);

            services.AddSingleton(transliterationSettings);
            services.AddSingleton(securitySettings);

            return services;
        }

        public static IServiceCollection AddCustomServices(
            this IServiceCollection services) => services
                .AddSingleton<IExonymsService, ExonymsService>()
                .AddSingleton<INameNormaliser, NameNormaliser>()
                .AddSingleton<INameConstructor, NameConstructor>()
                .AddSingleton<IGeoNamesGatherer, GeoNamesGatherer>()
                .AddSingleton<IWikiDataGatherer, WikiDataGatherer>()
                .AddTransient<ITransliterationApiClient, TransliterationApiClient>()
                .AddTransient<INuciApiClient>(provider => new NuciApiClient(provider.GetRequiredService<TransliterationSettings>().TransliterationApiBaseUrl));
    }
}
