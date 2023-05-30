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
        [TestCase("Abydos")]
        [TestCase("Horamabada")]
        [TestCase("Không Đồng")]
        public void GivenANameDoesNotHaveUnwantedWords_WhenNormalisingIt_ThenTheNameRemainsIntact(
            string name)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(name));

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
        [TestCase("Opactwo Tintern", "Tintern")]
        [TestCase("Opatija Tintern", "Tintern")]
        [TestCase("Tintern Abbey", "Tintern")]
        public void GivenANameContainsTheWordAbbey_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Ardal Kongtong", "Kongtong")]
        public void GivenANameContainsTheWordArea_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Āiruìbùníchéngbǎo", "Āiruìbùní")]
        [TestCase("Benteng Erebuni", "Erebuni")]
        [TestCase("Erebunifästningen", "Erebuni")]
        [TestCase("Fortăreața Erebuni", "Erebuni")]
        [TestCase("Fortezza di Erebuni", "Erebuni")]
        [TestCase("Fortikaĵo Erebuno", "Erebuno")]
        [TestCase("Pūrandar Chillā", "Pūrandar")]
        [TestCase("Purandar Fortress", "Purandar")]
        [TestCase("Pūrāndār Fūrt", "Pūrāndār")]
        [TestCase("Purantar Kōṭṭai", "Purantar")]
        [TestCase("Trdnjava Erebuni", "Erebuni")]
        public void GivenANameContainsTheWordFort_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Království Damot", "Damot")]
        public void GivenANameContainsTheWordKingdom_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Biara Kykkos", "Kykkos")]
        [TestCase("Kikkos monastrı", "Kikkos")]
        [TestCase("Klášter Tintern", "Tintern")]
        [TestCase("Klasztor Kykkos", "Kykkos")]
        [TestCase("Kykkos Monastery", "Kykkos")]
        [TestCase("Putna Monaĥejo", "Putna")]
        [TestCase("Samostan Kykkos", "Kykkos")]
        [TestCase("Tintern Abbey", "Tintern")]
        public void GivenANameContainsTheWordMoanstery_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Gemeente Enschede", "Enschede")]
        [TestCase("Överkalix udalerria", "Överkalix")]
        public void GivenANameContainsTheWordMunicipality_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Provintsiya Kurted", "Kurted")]
        [TestCase("Sarāburi Praviśya", "Sarāburi")]
        public void GivenANameContainsTheWordProvince_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("State of New York", "New York")]
        [TestCase("Statul California", "California")]
        public void GivenANameContainsTheWordState_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("fi", "Tinternin luostari", "Tintern")]
        [TestCase("fi", "Ylikainuun kunta", "Ylikainuu")]
        [TestCase("ja", "Saraburii Ken", "Saraburii")]
        [TestCase("ko", "Kungtunggu", "Kungtung")]
        [TestCase("ko", "Saraburiju", "Saraburi")]
        [TestCase("nan", "Khongtôngkhu", "Khongtông")]
        [TestCase("nan", "Överkalix chhī", "Överkalix")]
        [TestCase("zh", "Ājīkèkùlèhú", "Ājīkèkùlè")]
        [TestCase("zh", "Běi-biāo-fǔ", "Běibiāo")]
        [TestCase("zh", "Kōngdòngōu", "Kōngdòng")]
        [TestCase("zh", "Shǎngkǎlìkèsīshìzhēn", "Shǎngkǎlìkèsī")]
        public void GivenALanguageSpecificName_WhenNormalisingIt_ThenAllUnwantedWordsForThatLanguageAreRemoved(
            string languageCode,
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(languageCode, name), Is.EqualTo(expectedNormalisedName));
    }
}
