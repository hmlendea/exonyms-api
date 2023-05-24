using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using ExonymsAPI.Service.Models;
using Newtonsoft.Json.Linq;

namespace ExonymsAPI.Service.Gatherers
{
    public class WikiDataGatherer : IWikiDataGatherer
    {
        public const string DefaultNameLanguageCode = "en";

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
                }

                foreach (var label in labels)
                {
                    string languageCode = label.Key;
                    string name = (string)label.Value["value"];

                    if (name.Equals(location.DefaultName) &&
                        languageCode != DefaultNameLanguageCode)
                    {
                        continue;
                    }

                    location.Names.Add(languageCode, name);
                }

                foreach (var sitelink in sitelinks)
                {
                    string languageCode = sitelink.Key.Replace("wiki", "");
                    string name = (string)sitelink.Value["title"];

                    if (name.Equals(location.DefaultName) ||
                        location.Names.ContainsKey(languageCode))
                    {
                        continue;
                    }

                    location.Names.Add(languageCode, name);
                }
            }

            location.Names = location.Names
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);

            return location;
        }
    }
}
