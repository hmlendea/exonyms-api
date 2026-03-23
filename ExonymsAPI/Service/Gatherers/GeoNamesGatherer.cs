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
using NuciLog.Core;

namespace ExonymsAPI.Service.Gatherers
{
    public class GeoNamesGatherer(
        INameNormaliser nameNormaliser,
        ITransliterationApiClient transliterationApiClient,
        ILogger logger) : IGeoNamesGatherer
    {
        private static string GeoNamesRequestUrlFormat => "http://api.geonames.org/get?geonameId={0}&username=geonamesfreeaccountt";
        private static string DefaultNameLanguageCode => "en";
        private static string[] IgnoredLanguageCodes => ["link", "unlc", "wkdt"];

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

                logger.Debug(
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
        {Location location = new();

            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync(string.Format(GeoNamesRequestUrlFormat, geoNamesId));

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
