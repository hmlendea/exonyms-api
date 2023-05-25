using NUnit.Framework;
using ExonymsAPI.Service.Normalisers;

namespace ExonymsAPI.UnitTests.Service
{
    public class NameNormaliserTests
    {
        private INameNormaliser nameNormaliser;

        [SetUp]
        public void SetUp()
        {
            this.nameNormaliser = new NameNormaliser();
        }

        [Test]
        [TestCase(" Constantinople", "Constantinople")]
        [TestCase("Kreisfreie  Aachen", "Kreisfreie Aachen")]
        [TestCase("Madrid ", "Madrid")]
        public void GivenAName_WhenNormalisingIt_ThenTheWhitespacesAreTrimmed(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Category:Bucharest", "Bucharest")]
        [TestCase("Kategorie:freiburg", "freiburg")]
        public void GivenAName_WhenNormalisingIt_ThenOnlyTheNamePartIsReturned(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Abati Tyndyrn", "Tyndyrn")]
        [TestCase("Abaty Tyndyrn", "Tyndyrn")]
        [TestCase("abbaye de Tintern", "Tintern")]
        [TestCase("abbazia di Tintern", "Tintern")]
        [TestCase("Klášter Tintern", "Tintern")]
        [TestCase("Kykkos Monastery", "Kykkos")]
        [TestCase("Opactwo Tintern", "Tintern")]
        [TestCase("Opatija Tintern", "Tintern")]
        [TestCase("Putna Monaĥejo", "Putna")]
        [TestCase("Sarāburi Praviśya", "Sarāburi")]
        [TestCase("Tintern Abbey", "Tintern")]
        public void GivenAName_WhenNormalisingIt_ThenAllUnwantedWordsAreRemoved(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("fi", "Tinternin luostari", "Tintern")]
        [TestCase("ja", "Saraburii Ken", "Saraburii")]
        [TestCase("ko", "Saraburiju", "Saraburi")]
        [TestCase("zh", "Ājīkèkùlèhú", "Ājīkèkùlè")]
        [TestCase("zh", "Běi-biāo-fǔ", "Běibiāo")]
        public void GivenALanguageSpecificName_WhenNormalisingIt_ThenAllUnwantedWordsForThatLanguageAreRemoved(
            string languageCode,
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(languageCode, name), Is.EqualTo(expectedNormalisedName));
    }
}
