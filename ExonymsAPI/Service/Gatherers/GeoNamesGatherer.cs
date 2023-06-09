using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using ExonymsAPI.Service.Models;
using ExonymsAPI.Service.Normalisers;

namespace ExonymsAPI.Service.Gatherers
{
    public class GeoNamesGatherer : IGeoNamesGatherer
    {
        private static string DefaultNameLanguageCode => "en";
        private static string[] IgnoredLanguageCodes => new string[] { "link", "unlc", "wkdt" };

        INameNormaliser nameNormaliser;
        INameTransliterator nameTransliterator;

        public GeoNamesGatherer(
            INameNormaliser nameNormaliser,
            INameTransliterator nameTransliterator)
        {
            this.nameNormaliser = nameNormaliser;
            this.nameTransliterator = nameTransliterator;
        }

        public async Task<Location> Gather(string geoNamesId)
        {
            Location location = new Location();

            using (HttpClient client = new HttpClient())
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

                        string name = alternateNameElement.Value;

                        if (string.IsNullOrWhiteSpace(languageCode) ||
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
                }
            }

            return location;
        }
    }
}
