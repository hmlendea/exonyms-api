using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ExonymsAPI.Service.Gatherers;
using ExonymsAPI.Service.Models;
using ExonymsAPI.Service.Normalisers;

namespace ExonymsAPI.Service
{
    public class ExonymsService : IExonymsService
    {
        IGeoNamesGatherer geoNamesGatherer;
        IWikiDataGatherer wikiDataGatherer;
        INameTransliterator nameTransliterator;
        INameNormaliser nameNormaliser;

        public ExonymsService(
            IGeoNamesGatherer geoNamesGatherer,
            IWikiDataGatherer wikiDataGatherer,
            INameTransliterator nameTransliterator,
            INameNormaliser nameNormaliser)
        {
            this.geoNamesGatherer = geoNamesGatherer;
            this.wikiDataGatherer = wikiDataGatherer;
            this.nameTransliterator = nameTransliterator;
            this.nameNormaliser = nameNormaliser;
        }

        private IDictionary<string, IEnumerable<string>> languageFallbacks = new Dictionary<string, IEnumerable<string>>
        {
            { "bg", new List<string> { "ru", "uk" } },
            { "be", new List<string> { "ru", "uk", "bg" } },
            { "grc", new List<string> { "el" } },
            { "grc-dor", new List<string> { "grc" } },
            { "ru", new List<string> { "uk", "bg" } },
            { "uk", new List<string> { "ru", "bg" } }
        };

        public async Task<Location> Gather(string geoNamesId, string wikiDataId)
        {
            IList<Location> gatheredLocations = new List<Location>();

            if (!string.IsNullOrWhiteSpace(wikiDataId))
            {
                gatheredLocations.Add(await wikiDataGatherer.Gather(wikiDataId));
            }

            if (!string.IsNullOrWhiteSpace(geoNamesId))
            {
                gatheredLocations.Add(await geoNamesGatherer.Gather(geoNamesId));
            }

            Location location = new Location();

            foreach (Location gatheredLocation in gatheredLocations)
            {
                if (string.IsNullOrWhiteSpace(location.DefaultName))
                {
                    location.DefaultName = gatheredLocation.DefaultName;
                }

                foreach (var name in gatheredLocation.Names.Where(x => !location.Names.ContainsKey(x.Key)))
                {
                    location.Names.TryAdd(name.Key, name.Value);
                }
            }

            foreach (string languageToFallbackFrom in languageFallbacks.Keys)
            {
                foreach (string languageToFallbackTo in languageFallbacks[languageToFallbackFrom])
                {
                    if (location.Names.ContainsKey(languageToFallbackFrom))
                    {
                        break;
                    }

                    if (location.Names.ContainsKey(languageToFallbackTo))
                    {
                        Name name = new Name(location.Names[languageToFallbackTo].OriginalValue);

                        name.Value = await nameTransliterator.Transliterate(languageToFallbackFrom, name.Value);
                        name.Value = nameNormaliser.Normalise(languageToFallbackFrom, name.Value);

                        location.Names.Add(languageToFallbackFrom, name);
                    }
                }
            }

            location.Names = location.Names
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);

            return location;
        }
    }
}
