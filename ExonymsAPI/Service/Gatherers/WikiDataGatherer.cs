using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ExonymsAPI.Service.Models;
using ExonymsAPI.Service.Processors;
using Newtonsoft.Json.Linq;

namespace ExonymsAPI.Service.Gatherers
{
    public class WikiDataGatherer(
        INameNormaliser nameNormaliser,
        INameTransliterator nameTransliterator) : IWikiDataGatherer
    {
        public const string DefaultNameLanguageCode = "en";

        public async Task<Location> Gather(string wikiDataId)
        {
            Location location = new();

            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync($"https://wikidata.org/wiki/Special:EntityData/{wikiDataId}.json");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to retrieve WikiData entry for ID: {wikiDataId}");
                }

                string json = await response.Content.ReadAsStringAsync();

                JObject data = JObject.Parse(json);
                JObject entities = (JObject)data["entities"];
                JObject entity = (JObject)entities[wikiDataId];
                JObject labels = (JObject)entity["labels"];
                JObject sitelinks = (JObject)entity["sitelinks"];

                if (labels.TryGetValue(DefaultNameLanguageCode, out var defaultLabel))
                {
                    location.DefaultName = (string)defaultLabel["value"];
                    location.DefaultName = nameNormaliser.Normalise(DefaultNameLanguageCode, location.DefaultName);
                }

                foreach (var label in labels)
                {
                    string languageCode = label.Key;
                    Name name = new((string)label.Value["value"]);

                    if (Name.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    name.Value = await nameTransliterator.Transliterate(languageCode, name.OriginalValue);
                    name.Value = nameNormaliser.Normalise(languageCode, name.Value);

                    location.Names.Add(languageCode, name);
                }

                foreach (var sitelink in sitelinks)
                {
                    string languageCode = Regex.Replace(sitelink.Key, @"(news|quote|source|voyage|wiki)", "");

                    if (location.Names.ContainsKey(languageCode))
                    {
                        continue;
                    }

                    Name name = new((string)sitelink.Value["title"]);

                    name.Value = await nameTransliterator.Transliterate(languageCode, name.OriginalValue);
                    name.Value = nameNormaliser.Normalise(languageCode, name.Value);

                    if (name.Equals(location.DefaultName))
                    {
                        continue;
                    }

                    location.Names.Add(languageCode, name);
                }
            }

            return location;
        }
    }
}
