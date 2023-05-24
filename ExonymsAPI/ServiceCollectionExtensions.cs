using Microsoft.Extensions.DependencyInjection;

using ExonymsAPI.Service;
using ExonymsAPI.Service.Gatherers;
using ExonymsAPI.Service.Normalisers;

namespace ExonymsAPI
{
    public static class ServiceCollectionExtensions
    {
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
