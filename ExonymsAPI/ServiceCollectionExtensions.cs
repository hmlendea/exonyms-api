using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ExonymsAPI.Service;
using ExonymsAPI.Service.Gatherers;
using ExonymsAPI.Service.Normalisers;
using ExonymsAPI.Configuration;

namespace ExonymsAPI
{
    public static class ServiceCollectionExtensions
    {
        static TransliterationSettings transliterationSettings;

        public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            transliterationSettings = new TransliterationSettings();

            configuration.Bind(nameof(TransliterationSettings), transliterationSettings);

            services.AddSingleton(transliterationSettings);

            return services;
        }

        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IExonymsService, ExonymsService>()
                .AddSingleton<INameNormaliser, NameNormaliser>()
                .AddSingleton<INameTransliterator, NameTransliterator>()
                .AddSingleton<IGeoNamesGatherer, GeoNamesGatherer>()
                .AddSingleton<IWikiDataGatherer, WikiDataGatherer>();
        }
    }
}
