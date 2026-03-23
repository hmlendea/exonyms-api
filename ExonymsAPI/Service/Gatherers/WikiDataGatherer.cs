using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExonymsAPI.Client.TransliterationAPI;
using ExonymsAPI.Logging;
using ExonymsAPI.Service.Models;
using ExonymsAPI.Service.Processors;
using Newtonsoft.Json.Linq;
using NuciLog.Core;
using NuciWeb.HTTP;

namespace ExonymsAPI.Service.Gatherers
{
    public class WikiDataGatherer(
        INameNormaliser nameNormaliser,
        ITransliterationApiClient transliterationApiClient,
        ILogger logger) : IWikiDataGatherer
    {
        public const string DefaultNameLanguageCode = "en";

        public async Task<Location> Gather(string wikiDataId)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new LogInfo(MyLogInfoKey.WikiDataId, wikiDataId)
            ];

            logger.Info(
                MyOperation.GatherWikiDataExonyms,
                OperationStatus.Started,
                logInfos);

            try
            {
                Location location = await FetchLocation(wikiDataId);

                logger.Info(
                    MyOperation.GatherWikiDataExonyms,
                    OperationStatus.Success,
                    logInfos,
                    new LogInfo(MyLogInfoKey.DefaultName, location.DefaultName),
                    new LogInfo(MyLogInfoKey.Count, location.Names.Count));

                return location;
            }
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.GatherWikiDataExonyms,
                    OperationStatus.Failure,
                    ex,
                    logInfos);

                throw;
            }
        }

        async Task<Location> FetchLocation(string wikiDataId)
        {
            Location location = new();

            using (HttpClient client = HttpClientCreator.Create())
            {
                HttpResponseMessage response = await client.GetAsync($"https://wikidata.org/wiki/Special:EntityData/{wikiDataId}.json");

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to retrieve the WikiData entry for '{wikiDataId}': {response.StatusCode}");
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

                    name.Value = await transliterationApiClient.Transliterate(languageCode, name.OriginalValue);
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

                    name.Value = await transliterationApiClient.Transliterate(languageCode, name.OriginalValue);
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
