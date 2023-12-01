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
            { "be", new List<string> { "ru", "uk", "bg" } },
            { "bg", new List<string> { "ru", "uk" } },
            { "cu", new List<string> { "sr-ec", "bg", "mk", "ru", "uk" } },
            { "cv", new List<string> { "ru", "uk" } },
            { "grc-dor", new List<string> { "grc", "el", "pnt" } },
            { "grc", new List<string> { "grc-dor", "el", "pnt" } },
            { "kk", new List<string> { "ru", "bg", "uk" } },
            { "mk", new List<string> { "sr-ec", "bg", "ru", "uk" } },
            { "ru", new List<string> { "uk", "bg" } },
            { "sh", new List<string> { "sr-ec", "mk", "bg", "ru", "uk" } },
            { "sr-ec", new List<string> { "bg", "mk", "ru", "uk" } },
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
                if (location.Names.ContainsKey(languageToFallbackFrom))
                {
                    break;
                }

                foreach (string languageToFallbackTo in languageFallbacks[languageToFallbackFrom])
                {
                    if (!location.Names.ContainsKey(languageToFallbackTo))
                    {
                        continue;
                    }

                    string nameValue = location.Names[languageToFallbackTo].OriginalValue;
                    nameValue = await nameTransliterator.Transliterate(languageToFallbackTo, nameValue);
                    nameValue = nameNormaliser.Normalise(languageToFallbackTo, nameValue);

                    Name name = new Name(nameValue)
                    {
                        Comment = $"Based on language '{languageToFallbackTo}'"
                    };

                    location.Names.Add(languageToFallbackFrom, name);
                }
            }

            location.Names = location.Names
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);

            foreach (string language in location.Names.Keys)
            {
                if (string.IsNullOrWhiteSpace(location.Names[language].Value))
                {
                    continue;
                }

                if (!language.Equals(WikiDataGatherer.DefaultNameLanguageCode) &&
                    location.Names[language].Value.Equals(location.DefaultName))
                {
                    location.Names.Remove(language);
                }
            }

            return location;
        }
    }
}
