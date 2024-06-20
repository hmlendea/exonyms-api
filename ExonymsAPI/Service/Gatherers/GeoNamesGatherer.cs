using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using ExonymsAPI.Service.Models;
using ExonymsAPI.Service.Processors;

namespace ExonymsAPI.Service.Gatherers
{
    public class GeoNamesGatherer(
        INameNormaliser nameNormaliser,
        INameTransliterator nameTransliterator) : IGeoNamesGatherer
    {
        private static string DefaultNameLanguageCode => "en";
        private static string[] IgnoredLanguageCodes => ["link", "unlc", "wkdt"];

        public async Task<Location> Gather(string geoNamesId)
        {
            Location location = new();

            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync($"http://api.geonames.org/get?geonameId={geoNamesId}&username=geonamesfreeaccountt");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to retrieve GeoNames entry for ID: {geoNamesId}");
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

                        name.Value = await nameTransliterator.Transliterate(languageCode, name.Value);
                        name.Value = nameNormaliser.Normalise(languageCode, name.Value);

                        location.Names.Add(languageCode, name);
                    }
                }
            }

            return location;
        }
    }
}
