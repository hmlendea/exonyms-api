using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ExonymsAPI.Service.Gatherers;
using ExonymsAPI.Service.Models;
using ExonymsAPI.Service.Processors;

namespace ExonymsAPI.Service
{
    public class ExonymsService : IExonymsService
    {
        IGeoNamesGatherer geoNamesGatherer;
        IWikiDataGatherer wikiDataGatherer;
        INameConstructor nameConstructor;
        INameTransliterator nameTransliterator;
        INameNormaliser nameNormaliser;

        public ExonymsService(
            IGeoNamesGatherer geoNamesGatherer,
            IWikiDataGatherer wikiDataGatherer,
            INameConstructor nameConstructor,
            INameTransliterator nameTransliterator,
            INameNormaliser nameNormaliser)
        {
            this.geoNamesGatherer = geoNamesGatherer;
            this.wikiDataGatherer = wikiDataGatherer;
            this.nameConstructor = nameConstructor;
            this.nameTransliterator = nameTransliterator;
            this.nameNormaliser = nameNormaliser;
        }

        private IDictionary<string, IEnumerable<string>> languageFallbacks = new Dictionary<string, IEnumerable<string>>
        {
            { "be", new List<string> { "ru", "uk", "bg", "cv" } },
            { "bg", new List<string> { "ru", "uk", "be", "cv" } },
            { "cu", new List<string> { "sr-ec", "bg", "mk", "ru", "uk", "be", "cv" } },
            { "cv", new List<string> { "ru", "uk", "be", "bg" } },
            { "grc-dor", new List<string> { "grc", "el", "pnt" } },
            { "grc", new List<string> { "grc-dor", "el", "pnt" } },
            { "kk", new List<string> { "cv", "ru", "bg", "uk", "be" } },
            { "mk", new List<string> { "sr-ec", "bg", "ru", "uk", "be", "cv" } },
            { "ru", new List<string> { "uk", "bg", "be", "cv" } },
            { "sh", new List<string> { "sr", "sr-ec", "mk", "bg", "ru", "uk", "be", "cv" } },
            { "uk", new List<string> { "ru", "bg", "be", "cv" } }
        };

        private IDictionary<string, IEnumerable<string>> languagesToConstruct = new Dictionary<string, IEnumerable<string>>
        {
            { "gmh", new List<string> { "de" } }
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

            location = ConstructNames(location);
            location = RemoveRedundantExonyms(location);
            location = await ApplyFallbacks(location);
            location = RemoveRedundantExonyms(location);

            location.Names = location.Names
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);

            return location;
        }

        private async Task<Location> ApplyFallbacks(Location location)
        {
            foreach (string languageToFallbackFrom in languageFallbacks.Keys)
            {
                if (location.Names.ContainsKey(languageToFallbackFrom))
                {
                    continue;
                }

                foreach (string languageToFallbackTo in languageFallbacks[languageToFallbackFrom])
                {
                    if (!location.Names.ContainsKey(languageToFallbackTo))
                    {
                        continue;
                    }

                    Name name = new Name(location.Names[languageToFallbackTo].OriginalValue)
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

        private Location ConstructNames(Location location)
        {
            foreach (string language in languagesToConstruct.Keys)
            {
                if (location.Names.ContainsKey(language))
                {
                    continue;
                }

                foreach (string baseLanguage in languagesToConstruct[language])
                {
                    if (!location.Names.ContainsKey(baseLanguage))
                    {
                        continue;
                    }

                    Name name = new Name(location.Names[baseLanguage].OriginalValue)
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
