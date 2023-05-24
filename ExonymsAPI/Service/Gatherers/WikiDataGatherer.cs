using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ExonymsAPI.Service.Models;
using ExonymsAPI.Service.Normalisers;
using Newtonsoft.Json.Linq;

namespace ExonymsAPI.Service.Gatherers
{
    public class WikiDataGatherer : IWikiDataGatherer
    {
        public const string DefaultNameLanguageCode = "en";

        INameNormaliser nameNormaliser;
        INameTransliterator nameTransliterator;

        public WikiDataGatherer(
            INameNormaliser nameNormaliser,
            INameTransliterator nameTransliterator)
        {
            this.nameNormaliser = nameNormaliser;
            this.nameTransliterator = nameTransliterator;
        }

        public async Task<Location> Gather(string wikiDataId)
        {
            Location location = new Location();

            using (HttpClient client = new HttpClient())
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
                    string name = (string)label.Value["value"];

                    if (name.Equals(location.DefaultName) ||
                        string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    name = await nameTransliterator.Transliterate(languageCode, name);
                    name = nameNormaliser.Normalise(languageCode, name);

                    if (name.Equals(location.DefaultName) &&
                        languageCode != DefaultNameLanguageCode)
                    {
                        continue;
                    }

                    location.Names.Add(languageCode, name);
                }

                foreach (var sitelink in sitelinks)
                {
                    string languageCode = Regex.Replace(sitelink.Key, @"(news|quote|source|voyage|wiki)", "");

                    if (location.Names.ContainsKey(languageCode))
                    {
                        continue;
                    }

                    string name = (string)sitelink.Value["title"];

                    name = await nameTransliterator.Transliterate(languageCode, name);
                    name = nameNormaliser.Normalise(languageCode, name);

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
