using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExonymsAPI.Client.TransliterationAPI;
using ExonymsAPI.Logging;
using ExonymsAPI.Service;
using ExonymsAPI.Service.Gatherers;
using ExonymsAPI.Service.Models;
using ExonymsAPI.Service.Processors;
using Moq;
using NUnit.Framework;
using NuciLog.Core;

namespace ExonymsAPI.UnitTests.Service
{
    public class ExonymsServiceTests
    {
        private Mock<IGeoNamesGatherer> geoNamesGathererMock;
        private Mock<IWikiDataGatherer> wikiDataGathererMock;
        private Mock<INameConstructor> nameConstructorMock;
        private Mock<ITransliterationApiClient> transliterationApiClientMock;
        private Mock<INameNormaliser> nameNormaliserMock;
        private Mock<ILogger> loggerMock;
        private ExonymsService exonymsService;

        [SetUp]
        public void SetUp()
        {
            geoNamesGathererMock = new Mock<IGeoNamesGatherer>();
            wikiDataGathererMock = new Mock<IWikiDataGatherer>();
            nameConstructorMock = new Mock<INameConstructor>();
            transliterationApiClientMock = new Mock<ITransliterationApiClient>();
            nameNormaliserMock = new Mock<INameNormaliser>();
            loggerMock = new Mock<ILogger>();

            exonymsService = new ExonymsService(
                geoNamesGathererMock.Object,
                wikiDataGathererMock.Object,
                nameConstructorMock.Object,
                transliterationApiClientMock.Object,
                nameNormaliserMock.Object,
                loggerMock.Object);
        }

        [Test]
        public async Task GivenBothIdsAreEmpty_WhenGathering_ThenEmptyLocationIsReturned()
        {
            Location result = await exonymsService.Gather("", "");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.DefaultName, Is.Null.Or.Empty);
            Assert.That(result.Names, Is.Empty);
        }

        [Test]
        public async Task GivenOnlyGeoNamesId_WhenGathering_ThenGeoNamesLocationIsUsed()
        {
            Location geoNamesLocation = new("GeoNamesDefault")
            {
                Names = new Dictionary<string, Name>
                {
                    { "de", new Name("GermanName") { Value = "GermanValue" } }
                }
            };

            geoNamesGathererMock.Setup(x => x.Gather("12345")).ReturnsAsync(geoNamesLocation);

            Location result = await exonymsService.Gather("12345", "");

            Assert.That(result.DefaultName, Is.EqualTo("GeoNamesDefault"));
            Assert.That(result.Names.ContainsKey("de"), Is.True);
        }

        [Test]
        public async Task GivenOnlyWikiDataId_WhenGathering_ThenWikiDataLocationIsUsed()
        {
            Location wikiDataLocation = new("WikiDataDefault")
            {
                Names = new Dictionary<string, Name>
                {
                    { "fr", new Name("FrenchName") { Value = "FrenchValue" } }
                }
            };

            wikiDataGathererMock.Setup(x => x.Gather("Q123")).ReturnsAsync(wikiDataLocation);

            Location result = await exonymsService.Gather("", "Q123");

            Assert.That(result.DefaultName, Is.EqualTo("WikiDataDefault"));
            Assert.That(result.Names.ContainsKey("fr"), Is.True);
        }

        [Test]
        public async Task GivenBothIds_WhenGathering_ThenLocationsAreMerged()
        {
            Location geoNamesLocation = new("GeoNamesDefault")
            {
                Names = new Dictionary<string, Name>
                {
                    { "de", new Name("GermanName") { Value = "GermanValue" } }
                }
            };

            Location wikiDataLocation = new("WikiDataDefault")
            {
                Names = new Dictionary<string, Name>
                {
                    { "fr", new Name("FrenchName") { Value = "FrenchValue" } }
                }
            };

            geoNamesGathererMock.Setup(x => x.Gather("12345")).ReturnsAsync(geoNamesLocation);
            wikiDataGathererMock.Setup(x => x.Gather("Q123")).ReturnsAsync(wikiDataLocation);

            Location result = await exonymsService.Gather("12345", "Q123");

            // WikiData is processed first, so its default name takes precedence
            Assert.That(result.DefaultName, Is.EqualTo("WikiDataDefault"));
            Assert.That(result.Names.ContainsKey("de"), Is.True);
            Assert.That(result.Names.ContainsKey("fr"), Is.True);
        }

        [Test]
        public async Task GivenWikiDataHasDefaultNameAndGeoNamesDoesNot_WhenGathering_ThenWikiDataDefaultNameIsUsed()
        {
            Location geoNamesLocation = new("")
            {
                Names = new Dictionary<string, Name>
                {
                    { "de", new Name("GermanName") { Value = "GermanValue" } }
                }
            };

            Location wikiDataLocation = new("WikiDataDefault")
            {
                Names = new Dictionary<string, Name>
                {
                    { "fr", new Name("FrenchName") { Value = "FrenchValue" } }
                }
            };

            geoNamesGathererMock.Setup(x => x.Gather("12345")).ReturnsAsync(geoNamesLocation);
            wikiDataGathererMock.Setup(x => x.Gather("Q123")).ReturnsAsync(wikiDataLocation);

            Location result = await exonymsService.Gather("12345", "Q123");

            Assert.That(result.DefaultName, Is.EqualTo("WikiDataDefault"));
        }

        [Test]
        public async Task GivenDuplicateLanguageInBothSources_WhenGathering_ThenWikiDataTakesPrecedence()
        {
            Location geoNamesLocation = new("Default")
            {
                Names = new Dictionary<string, Name>
                {
                    { "de", new Name("GeoNamesGerman") { Value = "GeoNamesValue" } }
                }
            };

            Location wikiDataLocation = new("Default")
            {
                Names = new Dictionary<string, Name>
                {
                    { "de", new Name("WikiDataGerman") { Value = "WikiDataValue" } }
                }
            };

            geoNamesGathererMock.Setup(x => x.Gather("12345")).ReturnsAsync(geoNamesLocation);
            wikiDataGathererMock.Setup(x => x.Gather("Q123")).ReturnsAsync(wikiDataLocation);

            Location result = await exonymsService.Gather("12345", "Q123");

            // WikiData is processed first, so its names take precedence
            Assert.That(result.Names["de"].OriginalValue, Is.EqualTo("WikiDataGerman"));
        }

        [Test]
        public async Task GivenLanguageToConstruct_WhenGathering_ThenConstructedNameIsAdded()
        {
            Location wikiDataLocation = new("Default")
            {
                Names = new Dictionary<string, Name>
                {
                    { "de", new Name("GermanName") { Value = "GermanValue" } }
                }
            };

            wikiDataGathererMock.Setup(x => x.Gather("Q123")).ReturnsAsync(wikiDataLocation);
            nameConstructorMock.Setup(x => x.Construct("GermanValue", "gmh")).Returns("ConstructedGmh");

            Location result = await exonymsService.Gather("", "Q123");

            Assert.That(result.Names.ContainsKey("gmh"), Is.True);
            Assert.That(result.Names["gmh"].Value, Is.EqualTo("ConstructedGmh"));
            Assert.That(result.Names["gmh"].Comment, Does.Contain("Constructed"));
        }

        [Test]
        public async Task GivenLanguageAlreadyExists_WhenConstructing_ThenExistingNameIsKept()
        {
            Location wikiDataLocation = new("Default")
            {
                Names = new Dictionary<string, Name>
                {
                    { "de", new Name("GermanName") { Value = "GermanValue" } },
                    { "gmh", new Name("ExistingGmh") { Value = "ExistingValue" } }
                }
            };

            wikiDataGathererMock.Setup(x => x.Gather("Q123")).ReturnsAsync(wikiDataLocation);

            Location result = await exonymsService.Gather("", "Q123");

            Assert.That(result.Names["gmh"].Value, Is.EqualTo("ExistingValue"));
            nameConstructorMock.Verify(x => x.Construct(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GivenFallbackLanguageMissing_WhenApplyingFallbacks_ThenFallbackIsApplied()
        {
            Location wikiDataLocation = new("Default")
            {
                Names = new Dictionary<string, Name>
                {
                    { "ru", new Name("RussianName") { Value = "RussianValue" } }
                }
            };

            wikiDataGathererMock.Setup(x => x.Gather("Q123")).ReturnsAsync(wikiDataLocation);

            // Set up mocks for all languages that have "ru" as fallback
            string[] languagesWithRuFallback = ["ab", "be", "bg", "cu", "cv", "kk", "mk", "sh", "tg", "tt", "uk"];
            foreach (string lang in languagesWithRuFallback)
            {
                string capturedLang = lang;
                transliterationApiClientMock.Setup(x => x.Transliterate(capturedLang, "RussianName")).ReturnsAsync($"{capturedLang}Value");
                nameNormaliserMock.Setup(x => x.Normalise(capturedLang, $"{capturedLang}Value")).Returns($"{capturedLang}Normalised");
            }

            Location result = await exonymsService.Gather("", "Q123");

            // Verify at least one fallback language was added
            Assert.That(result.Names.Count, Is.GreaterThan(1));
            Assert.That(result.Names.Keys.Any(k => k != "ru"), Is.True);

            // Verify uk specifically if it was added
            if (result.Names.ContainsKey("uk"))
            {
                Assert.That(result.Names["uk"].Comment, Does.Contain("Based on language 'ru'"));
            }
        }

        [Test]
        public async Task GivenTransliterationReturnsSameValue_WhenApplyingFallbacks_ThenSecondTransliterationIsAttempted()
        {
            Location wikiDataLocation = new("Default")
            {
                Names = new Dictionary<string, Name>
                {
                    { "ru", new Name("RussianName") { Value = "RussianValue" } }
                }
            };

            wikiDataGathererMock.Setup(x => x.Gather("Q123")).ReturnsAsync(wikiDataLocation);

            // Set up mocks for all languages that have "ru" as fallback
            string[] languagesWithRuFallback = ["ab", "be", "bg", "cu", "cv", "kk", "mk", "sh", "tg", "tt", "uk"];
            foreach (string lang in languagesWithRuFallback)
            {
                transliterationApiClientMock.Setup(x => x.Transliterate(lang, "RussianName")).ReturnsAsync("RussianName");
                nameNormaliserMock.Setup(x => x.Normalise(lang, "RussianName")).Returns($"{lang}Normalised");
            }

            // Second transliteration from "ru" for "uk"
            transliterationApiClientMock.Setup(x => x.Transliterate("ru", "RussianName")).ReturnsAsync("UkrainianValue");
            nameNormaliserMock.Setup(x => x.Normalise("uk", "UkrainianValue")).Returns("UkrainianNormalised");

            Location result = await exonymsService.Gather("", "Q123");

            Assert.That(result.Names.ContainsKey("uk"), Is.True);
            transliterationApiClientMock.Verify(x => x.Transliterate("ru", "RussianName"), Times.AtLeastOnce);
        }

        [Test]
        public async Task GivenExonymEqualsDefaultName_WhenRemovingRedundant_ThenExonymIsRemoved()
        {
            Location wikiDataLocation = new("DefaultName")
            {
                Names = new Dictionary<string, Name>
                {
                    { "de", new Name("GermanName") { Value = "DefaultName" } },
                    { "fr", new Name("FrenchName") { Value = "FrenchValue" } }
                }
            };

            wikiDataGathererMock.Setup(x => x.Gather("Q123")).ReturnsAsync(wikiDataLocation);

            Location result = await exonymsService.Gather("", "Q123");

            Assert.That(result.Names.ContainsKey("de"), Is.False);
            Assert.That(result.Names.ContainsKey("fr"), Is.True);
        }

        [Test]
        public async Task GivenExonymEqualsDefaultNameInDefaultLanguage_WhenRemovingRedundant_ThenExonymIsKept()
        {
            Location wikiDataLocation = new("DefaultName")
            {
                Names = new Dictionary<string, Name>
                {
                    { "en", new Name("EnglishName") { Value = "DefaultName" } }
                }
            };

            wikiDataGathererMock.Setup(x => x.Gather("Q123")).ReturnsAsync(wikiDataLocation);

            Location result = await exonymsService.Gather("", "Q123");

            Assert.That(result.Names.ContainsKey("en"), Is.True);
        }

        [Test]
        public async Task GivenNames_WhenReturning_ThenNamesAreSortedAlphabetically()
        {
            Location wikiDataLocation = new("Default")
            {
                Names = new Dictionary<string, Name>
                {
                    { "zz", new Name("ZName") { Value = "ZValue" } },
                    { "aa", new Name("AName") { Value = "AValue" } },
                    { "mm", new Name("MName") { Value = "MValue" } }
                }
            };

            wikiDataGathererMock.Setup(x => x.Gather("Q123")).ReturnsAsync(wikiDataLocation);

            Location result = await exonymsService.Gather("", "Q123");

            List<string> keys = new List<string>(result.Names.Keys);
            Assert.That(keys, Is.Ordered);
        }

        [Test]
        public async Task GivenGathererThrows_WhenGathering_ThenExceptionIsRethrown()
        {
            geoNamesGathererMock.Setup(x => x.Gather("12345")).ThrowsAsync(new System.Exception("Test error"));

            Assert.ThrowsAsync<System.Exception>(async () => await exonymsService.Gather("12345", ""));
        }
    }
}