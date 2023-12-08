using NUnit.Framework;
using ExonymsAPI.Service.Processors;

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
        [TestCase("Basanija")]
        [TestCase("Bielina")]
        [TestCase("Birāṭanagara")]
        [TestCase("Biratnagara")]
        [TestCase("Bonchurch")]
        [TestCase("Bulgario")]
        [TestCase("Castrocaro")]
        [TestCase("Clariae")]
        [TestCase("Daesitiates")]
        [TestCase("Escitia Menor")]
        [TestCase("Horamabada")]
        [TestCase("Kastamonī́")]
        [TestCase("Kastamōnu")]
        [TestCase("Không Đồng")]
        [TestCase("Klosterneuburg")]
        [TestCase("Llanbadarn Fawr")]
        [TestCase("Llanddew")]
        [TestCase("Llangammarch")]
        [TestCase("Manastïr")]
        [TestCase("Manastır")]
        [TestCase("Monasterium")]
        [TestCase("Morava de Vest")]
        [TestCase("Oeseukemen")]
        [TestCase("Qūlja")]
        [TestCase("Rājyamahaḷ")]
        [TestCase("Sakaria")]
        [TestCase("Sitaun")]
        [TestCase("Solenoye")]
        [TestCase("Stolac")]
        [TestCase("Suðurland")]
        [TestCase("Toumanian")]
        [TestCase("Virāṭanagara")]
        [TestCase("Yuèpǔhú")]
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
        [TestCase("Yangadzha Airport", "Yangadzha")]
        public void GivenANameContainsTheWordAirport_WhenNormalisingIt_ThenOnlyTheNameRemains(
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
        [TestCase("Caisteal Chartley", "Chartley")]
        [TestCase("Castell Chartley", "Chartley")]
        [TestCase("Chartley Castle", "Chartley")]
        public void GivenANameContainsTheWordCastle_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Gurujiyashi", "Gurujiya")]
        [TestCase("Khulj khot", "Khulj")]
        [TestCase("Yainiṅg Nagaraṁ", "Yainiṅg")]
        [TestCase("Yeniṅga Śahar", "Yeniṅg")]
        [TestCase("Yiṉiṅ Nakaram", "Yiṉiṅ")]
        [TestCase("Yining City", "Yining")]
        [TestCase("Yining Shi", "Yining")]
        [TestCase("Yining Siti", "Yining")]
        [TestCase("Yining Siṭi", "Yining")]
        [TestCase("Yīníngshì", "Yīníng")]
        public void GivenANameContainsTheWordCity_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Gobnangou Cliffs", "Gobnangou")]
        public void GivenANameContainsTheWordCliff_WhenNormalisingIt_ThenOnlyTheNameRemains(
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
        [TestCase("Gorakhāmaṇḍalam", "Gorakhā")]
        public void GivenANameContainsTheWordDistrict_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Bahagian Garhwal", "Garhwal")]
        [TestCase("divisió de Garhwal", "Garhwal")]
        [TestCase("Division de Garhwal", "Garhwal")]
        [TestCase("División de Garhwal", "Garhwal")]
        [TestCase("divisione del Garhwal", "Garhwal")]
        [TestCase("Gaḍhavālavibhāgaḥ", "Gaḍhavāla")]
        [TestCase("Garhwal division", "Garhwal")]
        [TestCase("Garhwāl Mandal", "Garhwāl")]
        [TestCase("Gāṛōẏāla Bibhāga", "Gāṛōẏāla")]
        [TestCase("Kārvāl Kōṭṭam", "Kārvāl")]
        public void GivenANameContainsTheWordDivision_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Công quốc Holesten", "Holesten")]
        [TestCase("Doukáto tou Chόlstaïn", "Chόlstaïn")]
        [TestCase("Dugiaeth Holstein", "Holstein")]
        [TestCase("Dūqiyyah Hūlštāīn", "Hūlštāīn")]
        [TestCase("Hertigdömet Holstein", "Holstein")]
        [TestCase("Hertoochdoom Holstian", "Holstian")]
        [TestCase("Hertsogstvo Holshtayn", "Holshtayn")]
        [TestCase("Hertugdømmet Holsten", "Holsten")]
        [TestCase("Herzogtum Holstein", "Holstein")]
        [TestCase("Holšteino kunigaikštystė", "Holšteino")]
        [TestCase("Vojvodstvo Holštajn", "Holštajn")]
        public void GivenANameContainsTheWordDuchy_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));
        [Test]
        [TestCase("Emirlando Naĵrano", "Naĵrano")]
        public void GivenANameContainsTheWordEmirate_WhenNormalisingIt_ThenOnlyTheNameRemains(
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
        [TestCase("Hausa Kingdoms", "Hausa")]
        [TestCase("Království Damot", "Damot")]
        [TestCase("Kungadömet Hausa", "Hausa")]
        public void GivenANameContainsTheWordKingdom_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Jasıbay Gölü", "Jasıbay")]
        [TestCase("jezero Ngoring", "Ngoring")]
        [TestCase("lac Jasybay", "Jasybay")]
        [TestCase("Llac Jassibai", "Jassibai")]
        [TestCase("Ozero Zhasymbay", "Zhasymbay")]
        [TestCase("Ngoring Hu", "Ngoring")]
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
        [TestCase("Obabaieruntodōfuken", "Obabaierun")]
        public void GivenANameContainsTheWordPrefecture_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Manṭqah Naǧrān", "Naǧrān")]
        [TestCase("Manṭqah Naǧrān", "Naǧrān")]
        [TestCase("Minţaqat Najrān", "Najrān")]
        [TestCase("Najran Prāntaṁ", "Najran")]
        [TestCase("Ríu Chichikleya", "Chichikleya")]
        public void GivenANameContainsTheWordProvince_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Darjoi Sakarja", "Sakarja")]
        [TestCase("Gogēsa Shet'", "Gogēsa")]
        [TestCase("Gojeb Wenz", "Gojeb")]
        [TestCase("Kogin Gojeb", "Gojeb")]
        [TestCase("Mto Gojeb", "Gojeb")]
        [TestCase("Nhar Aīnhūl", "Aīnhūl")]
        [TestCase("Provintsiya Kurted", "Kurted")]
        [TestCase("Rio Sakarya", "Sakarya")]
        [TestCase("Ríu Inhul", "Inhul")]
        [TestCase("Rivero Sittaung", "Sittaung")]
        [TestCase("Sakarya Nehri", "Sakarya")]
        [TestCase("Sarāburi Praviśya", "Sarāburi")]
        [TestCase("Semani jõgi", "Semani")]
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
        [TestCase("Kota Nyêmo", "Nyêmo")]
        [TestCase("Naim Ṭaun", "Naim")]
        [TestCase("Naimō Paṭṭaṇaṁ", "Naimō")]
        [TestCase("Nayimō Ṭavuma", "Nayimō")]
        [TestCase("Naymo Tāun", "Naymo")]
        [TestCase("Nimō Ṭāuna", "Nimō")]
        [TestCase("Nímùxiāng", "Nímù")]
        [TestCase("Nyeme By", "Nyeme")]
        [TestCase("Nyemo Kasabası", "Nyemo")]
        [TestCase("Nyêmo Stad", "Nyêmo")]
        [TestCase("Nyêmo Town", "Nyêmo")]
        [TestCase("Nyimū Tāūn", "Nyimū")]
        public void GivenANameContainsTheWordTown_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Gòngjiǔbùxiāng", "Gòngjiǔbù")]
        public void GivenANameContainsTheWordTownship_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Golaghmuli Valley", "Golaghmuli")]
        [TestCase("Kōlākumuli Paḷḷattākku", "Kōlākumuli")]
        [TestCase("Wād Malwayah", "Malwayah")]
        [TestCase("Wādi Malwayah", "Malwayah")]
        [TestCase("Wādī Malwayah", "Malwayah")]
        public void GivenANameContainsTheWordValley_WhenNormalisingIt_ThenOnlyTheNameRemains(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("be", "Urdžarski raion", "Urdžar")]
        [TestCase("bg", "Urdzharski rayon", "Urdzhar")]
        [TestCase("cu", "Urdžarski rajon", "Urdžar")]
        [TestCase("cv", "Urdžarskij rajon", "Urdžar")]
        [TestCase("cz", "Holštýnské vévodství", "Holštýnsko")]
        [TestCase("en", "Valgorrera Tower", "Valgorrera")]
        [TestCase("en", "Yanchiwan National Natural Reserve", "Yanchiwan")]
        [TestCase("fi", "Gorkhakunta", "Gorkha")]
        [TestCase("fi", "Panchtharikunta", "Panchthar")]
        [TestCase("fi", "Tinternin luostari", "Tintern")]
        [TestCase("fi", "Ylikainuun kunta", "Ylikainuu")]
        [TestCase("hi", "Sitāng Nadi", "Sitāng")]
        [TestCase("hu", "Nadzsrán tartomány", "Nadzsrán")]
        [TestCase("it", "Torre Valgorrera", "Valgorrera")]
        [TestCase("ja", "Chiyatoribaraki", "Chiyatori")]
        [TestCase("ja", "Najiyuranshū", "Najiyuran")]
        [TestCase("ja", "Panchitarugun", "Panchitaru")]
        [TestCase("ja", "Saraburii Ken", "Saraburii")]
        [TestCase("kk", "Ürjar awdanı", "Ürjar")]
        [TestCase("ko", "Goreka Gun", "Goreka")]
        [TestCase("ko", "Julunghyeon", "Julung")]
        [TestCase("ko", "Kungtunggu", "Kungtung")]
        [TestCase("ko", "Najeuran Ju", "Najeuran")]
        [TestCase("ko", "Saraburiju", "Saraburi")]
        [TestCase("lv", "Nedžrānas mintaka", "Nedžrāna")]
        [TestCase("ml", "Bandar Nyêmo", "Nyêmo")]
        [TestCase("mr", "Najren Pardeś", "Najren")]
        [TestCase("ms", "Lapangan Terbang Yangadzha", "Yangadzha")]
        [TestCase("nan", "Khongtôngkhu", "Khongtông")]
        [TestCase("nan", "Överkalix chhī", "Överkalix")]
        [TestCase("pl", "Port lotniczy Jangadża", "Jangadża")]
        [TestCase("pl", "Sittaung Myit", "Sittaung")]
        [TestCase("ru", "Urdzharskiy rayon", "Urdzhar")]
        [TestCase("sv", "Gillbergs härad", "Gillberg")]
        [TestCase("uk", "Urdzharskyi raion", "Urdzhar")]
        [TestCase("vi", "Huyện Gorkha", "Gorkha")]
        [TestCase("vi", "Sông Sittaung", "Sittaung")]
        [TestCase("zh", "Ājīkèkùlèhú", "Ājīkèkùlè")]
        [TestCase("zh", "Bābāngxiāng", "Bābāng")]
        [TestCase("zh", "Běi-biāo-fǔ", "Běibiāo")]
        [TestCase("zh", "Gēzbohé", "Gēzbo")]
        [TestCase("zh", "Héěrsītàiyīngōng", "Héěrsītàiyīn")]
        [TestCase("zh", "Jiāwǎěrzhuān", "Jiāwǎěr")]
        [TestCase("zh", "Jiǔzhìxiàn", "Jiǔzhì")]
        [TestCase("zh", "Kōngdòngōu", "Kōngdòng")]
        [TestCase("zh", "Shǎngkǎlìkèsīshìzhēn", "Shǎngkǎlìkèsī")]
        [TestCase("zh", "Shēnzāzhēn", "Shēnzā")]
        [TestCase("zh", "Xīníngshì", "Xīníng")]
        [TestCase("zh", "Yánchíwāngūjízìránbǎohù", "Yánchíwān")]
        [TestCase("zh", "Yángbājǐngsì", "Yángbājǐng")]
        [TestCase("zh", "Yángkāngcūn", "Yángkāng")]
        [TestCase("zh", "Yuèpǔhúxiàn", "Yuèpǔ")]
        public void GivenALanguageSpecificName_WhenNormalisingIt_ThenAllUnwantedWordsForThatLanguageAreRemoved(
            string languageCode,
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(languageCode, name), Is.EqualTo(expectedNormalisedName));
    }
}
