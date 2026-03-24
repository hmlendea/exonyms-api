using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using ExonymsAPI.Client.TransliterationAPI;
using ExonymsAPI.Logging;
using ExonymsAPI.Service.Models;
using ExonymsAPI.Service.Processors;
using NuciExtensions;
using NuciLog.Core;

namespace ExonymsAPI.Service.Gatherers
{
    public class GeoNamesGatherer(
        INameNormaliser nameNormaliser,
        ITransliterationApiClient transliterationApiClient,
        ILogger logger) : IGeoNamesGatherer
    {
        private static string GeoNamesRequestEndpointFormat => "https://api.geonames.org/get?geonameId={0}&username={1}";
        private static string DefaultNameLanguageCode => "en";
        private static string[] IgnoredLanguageCodes => ["link", "unlc", "wkdt"];

        readonly IEnumerable<string> usernames =
        [
            "geonamesfreeaccountt", "freacctest1", "freacctest2", "commando.gaztons", "berestesievici", "izvoli.prostagma",
            "gesturioso", "random.name.ftw", "b75268", "b75375", "b75445", "b75445",
            "botu0", "botu1", "botu2", "botu3", "botu4", "botu5", "botu6", "botu7", "botu8", "botu9",
            "elBot0", "elBot1", "elBot2", "elBot3", "elBot4", "elBot5", "elBot6", "elBot7", "elBot8", "elBot9",
            "botman0", "botman1", "botman2", "botman3", "botman4", "botman5", "botman6", "botman7", "botman8", "botman9",
            "botean0", "botean1", "botean2", "botean3", "botean4", "botean5"
        ];

        public async Task<Location> Gather(string geoNamesId)
        {
            IEnumerable<LogInfo> logInfos =
            [
                new LogInfo(MyLogInfoKey.GeoNamesId, geoNamesId)
            ];

            logger.Info(
                MyOperation.GatherGeoNamesExonyms,
                OperationStatus.Started,
                logInfos);

            try
            {
                Location location = await FetchLocation(geoNamesId);

                logger.Info(
                    MyOperation.GatherGeoNamesExonyms,
                    OperationStatus.Success,
                    logInfos,
                    new LogInfo(MyLogInfoKey.DefaultName, location.DefaultName),
                    new LogInfo(MyLogInfoKey.Count, location.Names.Count));

                return location;
            }
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.GatherGeoNamesExonyms,
                    OperationStatus.Failure,
                    ex,
                    logInfos);

                throw;
            }
        }

        async Task<Location> FetchLocation(string geoNamesId)
        {
            Location location = new();

            using HttpClient client = new();
            {
                string geoNamesEndpoint = string.Format(
                    GeoNamesRequestEndpointFormat,
                    geoNamesId,
                    usernames.GetRandomElement());

                HttpResponseMessage response = await client.GetAsync(geoNamesEndpoint);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to retrieve the GeoNames entry for '{geoNamesId}': {response.StatusCode}");
                }

                string xml = await response.Content.ReadAsStringAsync();

                // Parse the XML response
                XDocument doc = XDocument.Parse(xml);
                XElement geonameElement = doc.Root;

                // Extract the alternate names
                IEnumerable<XElement> alternateNameElements = geonameElement.Elements("alternateName");

                location.DefaultName = (string)geonameElement.Element("name");
                location.DefaultName = nameNormaliser.Normalise(DefaultNameLanguageCode, location.DefaultName);

                if (alternateNameElements is not null)
                {
                    foreach (XElement alternateNameElement in alternateNameElements)
                    {
                        string languageCode = alternateNameElement.Attribute("lang")?.Value;

                        if (string.IsNullOrWhiteSpace(languageCode) ||
                            location.Names.ContainsKey(languageCode) ||
                            IgnoredLanguageCodes.Contains(languageCode))
                        {
                            continue;
                        }

                        Name name = new(alternateNameElement.Value);

                        if (string.IsNullOrWhiteSpace(languageCode) ||
                            Name.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        name.Value = await transliterationApiClient.Transliterate(languageCode, name.Value);
                        name.Value = nameNormaliser.Normalise(languageCode, name.Value);

                        location.Names.Add(languageCode, name);
                    }
                }
            }

            return location;
        }
    }
}
