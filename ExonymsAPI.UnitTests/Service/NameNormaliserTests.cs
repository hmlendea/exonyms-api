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
        [TestCase("Bielina")]
        [TestCase("Bonchurch")]
        [TestCase("Horamabada")]
        [TestCase("Không Đồng")]
        [TestCase("Klosterneuburg")]
        [TestCase("Solenoye")]
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
        [TestCase("Catégorie:Szeged", "Szeged")]
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
        [TestCase("Comté Gävleborg", "Gävleborg")]
        [TestCase("Condado Gävleborg", "Gävleborg")]
        [TestCase("Condado han Gävleborg", "Gävleborg")]
        [TestCase("Contea Jigzhi", "Jigzhi")]
        [TestCase("Daerah Gävleborg", "Gävleborg")]
        [TestCase("Gävleborg Comitatus", "Gävleborg")]
        [TestCase("Gävleborg Coonty", "Gävleborg")]
        [TestCase("Gävleborg ili", "Gävleborg")]
        [TestCase("Gävleborg koān", "Gävleborg")]
        [TestCase("Gävleborg Kōan", "Gävleborg")]
        [TestCase("Gävleborg megye", "Gävleborg")]
        [TestCase("Gävleborgeko konderria", "Gävleborg")]
        [TestCase("Gävleborgga leatna", "Gävleborg")]
        [TestCase("Gävleborgi lään", "Gävleborg")]
        [TestCase("Gävleborgin lääni", "Gävleborg")]
        [TestCase("Gävleborgs län", "Gävleborg")]
        [TestCase("Gävleborgs Län", "Gävleborg")]
        [TestCase("Jēvleborjas lēne", "Jēvleborja")]
        [TestCase("Jigzhi Xian", "Jigzhi")]
        [TestCase("Jiǔzhìxiàn", "Jiǔzhì")]
        [TestCase("Județul Hunedoara", "Hunedoara")]
        [TestCase("Komēteía Phouchái", "Phouchái")]
        [TestCase("Komīteía Fouchái", "Fouchái")]
        [TestCase("Komtio Gävleborg", "Gävleborg")]
        [TestCase("Konteth Gävleborg", "Gävleborg")]
        [TestCase("Lehn Gävleborg", "Gävleborg")]
        [TestCase("Sir Gävleborg", "Gävleborg")]
        [TestCase("Xian Jigzhi", "Jigzhi")]
        public void GivenANameContainsTheWordCounty_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Bygdeån piiri", "Bygdeå")]
        public void GivenANameContainsTheWordDistrict_WhenNormalisingIt_ThenOnlyTheNameRemains(
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
        [TestCase("Jasıbay Gölü", "Jasıbay")]
        [TestCase("lac Jasybay", "Jasybay")]
        [TestCase("Llac Jassibai", "Jassibai")]
        [TestCase("Ozero Zhasymbay", "Zhasymbay")]
        public void GivenANameContainsTheWordLake_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Bōduōnítèsàhóu", "Bōduōnítèsà")]
        [TestCase("Marchesato Bodonitsa", "Bodonitsa")]
        [TestCase("Marchezà Bodonitsa", "Bodonitsa")]
        [TestCase("Markgrafschaft Boudonitza", "Boudonitza")]
        [TestCase("Markgrafstvo Bodonitsa", "Bodonitsa")]
        [TestCase("Markgrevskapet Bodonitsa", "Bodonitsa")]
        [TestCase("Marquesado Bodonitsa", "Bodonitsa")]
        [TestCase("Marquesat Bodonitza", "Bodonitza")]
        [TestCase("Marquisat Bodonitza", "Bodonitza")]
        [TestCase("Marquisate Bodonitsa", "Bodonitsa")]
        public void GivenANameContainsTheWordMarquisate_WhenNormalisingIt_ThenOnlyTheNameRemains(
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
        [TestCase("Tupkaragan Peninsula", "Tupkaragan")]
        [TestCase("Tüpqaraghan Tübegi", "Tüpqaraghan")]
        public void GivenANameContainsTheWordPeninsula_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Ríu Chichikleya", "Chichikleya")]
        public void GivenANameContainsTheWordProvince_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Nhar Aīnhūl", "Aīnhūl")]
        [TestCase("Provintsiya Kurted", "Kurted")]
        [TestCase("Ríu Inhul", "Inhul")]
        [TestCase("Sarāburi Praviśya", "Sarāburi")]
        [TestCase("Yīněrhé", "Yīněr")]
        public void GivenANameContainsTheWordRiver_WhenNormalisingIt_ThenOnlyTheNameRemains(
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
