using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ExonymsAPI.Service.Gatherers;
using ExonymsAPI.Service.Models;
using ExonymsAPI.Service.Processors;

namespace ExonymsAPI.Service
{
    public class ExonymsService(
        IGeoNamesGatherer geoNamesGatherer,
        IWikiDataGatherer wikiDataGatherer,
        INameConstructor nameConstructor,
        INameTransliterator nameTransliterator,
        INameNormaliser nameNormaliser) : IExonymsService
    {
        private readonly IDictionary<string, IEnumerable<string>> languageFallbacks = new Dictionary<string, IEnumerable<string>>
        {
            { "ab", new List<string> { "ru", "uk", "be", "tg", "kk", "tt", "cv", "bg", "mk", "sr-ec", "cu" } },
            { "be", new List<string> { "ru", "uk", "cu", "bg", "mk", "sr-ec", "tt", "kk", "cv", "tg", "ab" } },
            { "bg", new List<string> { "ru", "mk", "sr-ec", "cu", "uk", "be", "tt", "cv", "kk", "tg", "ab" } },
            { "cu", new List<string> { "ru", "bg", "uk", "be", "mk", "sr-ec", "tt", "kk", "cv", "tg", "ab" } },
            { "cv", new List<string> { "ru", "tt", "kk", "uk", "be", "bg", "tg", "mk", "sr-ec", "cu", "ab" } },
            { "grc-dor", new List<string> { "grc", "el", "pnt" } },
            { "grc", new List<string> { "grc-dor", "el", "pnt" } },
            { "kk", new List<string> { "ru", "tt", "cv", "uk", "be", "tg", "bg", "mk", "sr-ec", "cu", "ab" } },
            { "mk", new List<string> { "bg", "ru", "sr-ec", "cu", "uk", "be", "tt", "cv", "kk", "tg", "ab" } },
            { "ru", new List<string> { "uk", "be", "cu", "bg", "mk", "sr-ec", "tt", "kk", "cv", "tg", "ab" } },
            { "sh", new List<string> { "sr", "sr-ec", "ru", "bg", "mk", "cu", "uk", "be", "tt", "cv", "kk", "tg", "ab" } },
            { "tg", new List<string> { "ru", "kk", "tt", "cv", "uk", "be", "bg", "mk", "sr-ec", "cu", "ab" } },
            { "tt", new List<string> { "ru", "kk", "cv", "uk", "be", "bg", "tg", "mk", "sr-ec", "cu", "ab" } },
            { "uk", new List<string> { "ru", "be", "cu", "bg", "mk", "sr-ec", "tt", "kk", "cv", "tg", "ab" } }
        };

        private readonly IDictionary<string, IEnumerable<string>> languagesToConstruct = new Dictionary<string, IEnumerable<string>>
        {
            { "gmh", new List<string> { "de" } }
        };

        public async Task<Location> Gather(string geoNamesId, string wikiDataId)
        {
            IList<Location> gatheredLocations = [];

            if (!string.IsNullOrWhiteSpace(wikiDataId))
            {
                gatheredLocations.Add(await wikiDataGatherer.Gather(wikiDataId));
            }

            if (!string.IsNullOrWhiteSpace(geoNamesId))
            {
                gatheredLocations.Add(await geoNamesGatherer.Gather(geoNamesId));
            }

            Location location = new();

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

            location = ConstructNames(location);
            location = await ApplyFallbacks(location);
            location = RemoveRedundantExonyms(location);

            location.Names = location.Names
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);

            return location;
        }

        private async Task<Location> ApplyFallbacks(Location location)
        {
            foreach (string languageToFallbackFrom in languageFallbacks.Keys.Where(l => !location.Names.ContainsKey(l)))
            {
                foreach (string languageToFallbackTo in languageFallbacks[languageToFallbackFrom].Where(location.Names.ContainsKey))
                {
                    Name name = new(location.Names[languageToFallbackTo].OriginalValue)
                    {
                        Comment = $"Based on language '{languageToFallbackTo}'"
                    };

                    name.Value = await nameTransliterator.Transliterate(languageToFallbackFrom, name.OriginalValue);

                    if (name.Value.Equals(name.OriginalValue))
                    {
                        name.Value = await nameTransliterator.Transliterate(languageToFallbackTo, name.OriginalValue);
                    }

                    name.Value = nameNormaliser.Normalise(languageToFallbackTo, name.Value);

                    location.Names.Add(languageToFallbackFrom, name);
                    break;
                }
            }

            return location;
        }

        private Location RemoveRedundantExonyms(Location location)
        {
            foreach (string language in location.Names.Keys.Where(l => !string.IsNullOrWhiteSpace(location.Names[l].Value)))
            {
                if (!language.Equals(WikiDataGatherer.DefaultNameLanguageCode) &&
                    location.Names[language].Value.Equals(location.DefaultName))
                {
                    location.Names.Remove(language);
                }
            }

            return location;
        }

        private Location ConstructNames(Location location)
        {
            foreach (string language in languagesToConstruct.Keys.Where(l => !location.Names.ContainsKey(l)))
            {
                foreach (string baseLanguage in languagesToConstruct[language].Where(location.Names.ContainsKey))
                {
                    Name name = new(location.Names[baseLanguage].OriginalValue)
                    {
                        Comment = $"Constructed. Based on language '{baseLanguage}'",
                        Value = nameConstructor.Construct(location.Names[baseLanguage].Value, language)
                    };

                    location.Names.Add(language, name);
                    break;
                }
            }

            return location;
        }
    }
}
