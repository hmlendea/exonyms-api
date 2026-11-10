using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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
    public sealed class WikiDataGathererTests
    {
        private Mock<INameNormaliser> nameNormaliserMock;
        private Mock<ITransliterationApiClient> transliterationApiClientMock;
        private Mock<ILogger> loggerMock;
        private WikiDataGatherer wikiDataGatherer;

        [SetUp]
        public void SetUp()
        {
            nameNormaliserMock = new Mock<INameNormaliser>();
            transliterationApiClientMock = new Mock<ITransliterationApiClient>();
            loggerMock = new Mock<ILogger>();

            wikiDataGatherer = new WikiDataGatherer(
                nameNormaliserMock.Object,
                transliterationApiClientMock.Object,
                loggerMock.Object);
        }

        [Test]
        public void GivenDefaultNameLanguageCode_WhenChecking_ThenItIsEn()
        {
            Assert.That(WikiDataGatherer.DefaultNameLanguageCode, Is.EqualTo("en"));
        }

        [Test]
        public void GivenJsonWithLabels_WhenParsing_ThenNamesAreExtracted()
        {
            string json = @"{
                ""entities"": {
                    ""Q123"": {
                        ""labels"": {
                            ""en"": { ""value"": ""Test City"" },
                            ""de"": { ""value"": ""Test Stadt"" },
                            ""fr"": { ""value"": ""Test Ville"" }
                        },
                        ""sitelinks"": {}
                    }
                }
            }";

            JsonNode data = JsonNode.Parse(json);
            JsonObject entities = data["entities"].AsObject();
            JsonObject entity = entities["Q123"].AsObject();
            JsonObject labels = entity["labels"].AsObject();

            Assert.That(labels.Count, Is.EqualTo(3));
            Assert.That(labels["en"]["value"].GetValue<string>(), Is.EqualTo("Test City"));
            Assert.That(labels["de"]["value"].GetValue<string>(), Is.EqualTo("Test Stadt"));
            Assert.That(labels["fr"]["value"].GetValue<string>(), Is.EqualTo("Test Ville"));
        }

        [Test]
        public void GivenJsonWithSitelinks_WhenParsing_ThenSitelinksAreExtracted()
        {
            string json = @"{
                ""entities"": {
                    ""Q123"": {
                        ""labels"": {
                            ""en"": { ""value"": ""Test City"" }
                        },
                        ""sitelinks"": {
                            ""dewiki"": { ""title"": ""Test Stadt"" },
                            ""frwiki"": { ""title"": ""Test Ville"" },
                            ""enwiki"": { ""title"": ""Test City"" }
                        }
                    }
                }
            }";

            JsonNode data = JsonNode.Parse(json);
            JsonObject entities = data["entities"].AsObject();
            JsonObject entity = entities["Q123"].AsObject();
            JsonObject sitelinks = entity["sitelinks"].AsObject();

            Assert.That(sitelinks.Count, Is.EqualTo(3));
            Assert.That(sitelinks["dewiki"]["title"].GetValue<string>(), Is.EqualTo("Test Stadt"));
            Assert.That(sitelinks["frwiki"]["title"].GetValue<string>(), Is.EqualTo("Test Ville"));
        }

        [Test]
        public void GivenSitelinkKey_WhenExtractingLanguageCode_ThenSuffixIsRemoved()
        {
            string[] sitelinkKeys = { "dewiki", "frwiki", "enwiki", "dewikinews", "frwikiquote", "enwikisource", "devoyage" };
            string[] expectedLanguages = { "de", "fr", "en", "de", "fr", "en", "de" };

            for (int i = 0; i < sitelinkKeys.Length; i++)
            {
                string languageCode = System.Text.RegularExpressions.Regex.Replace(
                    sitelinkKeys[i],
                    @"(news|quote|source|voyage|wiki)",
                    "");

                Assert.That(languageCode, Is.EqualTo(expectedLanguages[i]));
            }
        }

        [Test]
        public void GivenJsonWithoutLabels_WhenParsing_ThenEmptyLabels()
        {
            string json = @"{
                ""entities"": {
                    ""Q123"": {
                        ""labels"": {},
                        ""sitelinks"": {}
                    }
                }
            }";

            JsonNode data = JsonNode.Parse(json);
            JsonObject entities = data["entities"].AsObject();
            JsonObject entity = entities["Q123"].AsObject();
            JsonObject labels = entity["labels"].AsObject();

            Assert.That(labels.Count, Is.EqualTo(0));
        }

        [Test]
        public void GivenJsonWithoutEntity_WhenParsing_ThenNullEntity()
        {
            string json = @"{
                ""entities"": {}
            }";

            JsonNode data = JsonNode.Parse(json);
            JsonObject entities = data["entities"].AsObject();

            Assert.That(entities["Q123"], Is.Null);
        }

        [Test]
        public void GivenLabelWithEmptyValue_WhenChecking_ThenIsNullOrWhiteSpaceReturnsTrue()
        {
            Name name = new("");
            Assert.That(Name.IsNullOrWhiteSpace(name), Is.True);

            name = new("   ");
            Assert.That(Name.IsNullOrWhiteSpace(name), Is.True);

            name = null;
            Assert.That(Name.IsNullOrWhiteSpace(name), Is.True);

            name = new("Valid");
            Assert.That(Name.IsNullOrWhiteSpace(name), Is.False);
        }
    }
}