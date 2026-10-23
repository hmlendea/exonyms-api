using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NuciLog.Core;
using NuciWeb.HTTP;

using ExonymsAPI.Client.TransliterationAPI;
using ExonymsAPI.Logging;
using ExonymsAPI.Service.Models;
using ExonymsAPI.Service.Processors;

namespace ExonymsAPI.Service.Gatherers
{
    public sealed class WikiDataGatherer(
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

        private async Task<Location> FetchLocation(string wikiDataId)
        {
            Location location = new();

            using HttpClient client = HttpClientCreator.Create();

            HttpResponseMessage response = await client.GetAsync($"https://wikidata.org/wiki/Special:EntityData/{wikiDataId}.json");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to retrieve the WikiData entry for '{wikiDataId}': {response.StatusCode}");
            }

            string json = await response.Content.ReadAsStringAsync();

            JsonNode data = JsonNode.Parse(json);
            JsonObject entities = data["entities"].AsObject();
            JsonObject entity = entities[wikiDataId].AsObject();
            JsonObject labels = entity["labels"].AsObject();
            JsonObject sitelinks = entity["sitelinks"].AsObject();

            if (labels.ContainsKey(DefaultNameLanguageCode))
            {
                JsonNode defaultLabel = labels[DefaultNameLanguageCode];
                location.DefaultName = defaultLabel["value"].GetValue<string>();
                location.DefaultName = nameNormaliser.Normalise(DefaultNameLanguageCode, location.DefaultName);
            }

            foreach (var label in labels)
            {
                string languageCode = label.Key;
                Name name = new(label.Value["value"].GetValue<string>());

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

                Name name = new(sitelink.Value["title"].GetValue<string>());

                name.Value = await transliterationApiClient.Transliterate(languageCode, name.OriginalValue);
                name.Value = nameNormaliser.Normalise(languageCode, name.Value);

                if (name.Equals(location.DefaultName))
                {
                    continue;
                }

                location.Names.Add(languageCode, name);
            }

            return location;
        }
    }
}
