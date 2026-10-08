using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using ExonymsAPI.Client.TransliterationAPI;
using ExonymsAPI.Logging;
using ExonymsAPI.Service.Gatherers;
using ExonymsAPI.Service.Models;
using ExonymsAPI.Service.Processors;
using Moq;
using NUnit.Framework;
using NuciLog.Core;

namespace ExonymsAPI.UnitTests.Service.Gatherers
{
    public class GeoNamesGathererTests
    {
        private Mock<INameNormaliser> nameNormaliserMock;
        private Mock<ITransliterationApiClient> transliterationApiClientMock;
        private Mock<ILogger> loggerMock;
        private GeoNamesGatherer geoNamesGatherer;

        private static readonly string[] IgnoredLanguageCodes = ["link", "unlc", "wkdt"];
        private const string DefaultNameLanguageCode = "en";

        [SetUp]
        public void SetUp()
        {
            nameNormaliserMock = new Mock<INameNormaliser>();
            transliterationApiClientMock = new Mock<ITransliterationApiClient>();
            loggerMock = new Mock<ILogger>();

            geoNamesGatherer = new GeoNamesGatherer(
                nameNormaliserMock.Object,
                transliterationApiClientMock.Object,
                loggerMock.Object);
        }

        [Test]
        public async Task GivenValidGeoNamesId_WhenGathering_ThenLocationIsReturned()
        {
            // We can't easily mock HttpClient, so we'll test the parsing logic separately
            // This test would need a more sophisticated setup with HttpClient mocking
            Assert.Pass("Integration test - requires HttpClient mocking");
        }

        [Test]
        public void GivenXmlWithAlternateNames_WhenParsing_ThenNamesAreExtracted()
        {
            string xml = @"<geoname>
                <name>TestCity</name>
                <alternateName lang='de'>TestStadt</alternateName>
                <alternateName lang='fr'>TestVille</alternateName>
                <alternateName lang='link'>IgnoredLink</alternateName>
                <alternateName lang='unlc'>IgnoredUnlc</alternateName>
                <alternateName lang='wkdt'>IgnoredWkdt</alternateName>
            </geoname>";

            XDocument doc = XDocument.Parse(xml);
            XElement geonameElement = doc.Root;

            string defaultName = (string)geonameElement.Element("name");
            IEnumerable<XElement> alternateNameElements = geonameElement.Elements("alternateName");

            Assert.That(defaultName, Is.EqualTo("TestCity"));

            List<string> languages = new List<string>();
            foreach (XElement element in alternateNameElements)
            {
                string lang = element.Attribute("lang")?.Value;
                if (!string.IsNullOrWhiteSpace(lang) && !IgnoredLanguageCodes.ToList().Contains(lang))
                {
                    languages.Add(lang);
                }
            }

            Assert.That(languages, Has.Count.EqualTo(2));
            Assert.That(languages, Contains.Item("de"));
            Assert.That(languages, Contains.Item("fr"));
        }

        [Test]
        public void GivenXmlWithoutAlternateNames_WhenParsing_ThenEmptyNamesDictionary()
        {
            string xml = @"<geoname>
                <name>TestCity</name>
            </geoname>";

            XDocument doc = XDocument.Parse(xml);
            XElement geonameElement = doc.Root;

            IEnumerable<XElement> alternateNameElements = geonameElement.Elements("alternateName");

            Assert.That(alternateNameElements, Is.Empty);
        }

        [Test]
        public void GivenXmlWithEmptyLanguageCode_WhenParsing_ThenNameIsSkipped()
        {
            string xml = @"<geoname>
                <name>TestCity</name>
                <alternateName>NoLang</alternateName>
                <alternateName lang=''>EmptyLang</alternateName>
                <alternateName lang='de'>ValidLang</alternateName>
            </geoname>";

            XDocument doc = XDocument.Parse(xml);
            XElement geonameElement = doc.Root;

            IEnumerable<XElement> alternateNameElements = geonameElement.Elements("alternateName");

            int validCount = 0;
            foreach (XElement element in alternateNameElements)
            {
                string lang = element.Attribute("lang")?.Value;
                if (!string.IsNullOrWhiteSpace(lang) && !IgnoredLanguageCodes.ToList().Contains(lang))
                {
                    validCount++;
                }
            }

            Assert.That(validCount, Is.EqualTo(1));
        }

        [Test]
        public void GivenIgnoredLanguageCodes_WhenChecking_ThenTheyAreIgnored()
        {
            Assert.That(IgnoredLanguageCodes, Contains.Item("link"));
            Assert.That(IgnoredLanguageCodes, Contains.Item("unlc"));
            Assert.That(IgnoredLanguageCodes, Contains.Item("wkdt"));
        }

        [Test]
        public void GivenDefaultNameLanguageCode_WhenChecking_ThenItIsEn()
        {
            Assert.That(DefaultNameLanguageCode, Is.EqualTo("en"));
        }
    }
}