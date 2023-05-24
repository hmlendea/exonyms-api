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
        [TestCase("Abati Tyndyrn", "Tyndyrn")]
        [TestCase("Abaty Tyndyrn", "Tyndyrn")]
        [TestCase("abbaye de Tintern", "Tintern")]
        [TestCase("abbazia di Tintern", "Tintern")]
        [TestCase("Klášter Tintern", "Tintern")]
        [TestCase("Opactwo Tintern", "Tintern")]
        [TestCase("Opatija Tintern", "Tintern")]
        [TestCase("Tintern Abbey", "Tintern")]
        public void GivenAName_WhenNormalisingIt_ThenAllUnwantedWordsAreRemoved(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise(string.Empty, name), Is.EqualTo(expectedNormalisedName));

        [Test]
        [TestCase("Tinternin luostari", "Tintern")]
        public void GivenAFinnishName_WhenNormalisingIt_ThenAllUnwantedWordsAreRemoved(
            string name,
            string expectedNormalisedName)
            => Assert.That(nameNormaliser.Normalise("fi", name), Is.EqualTo(expectedNormalisedName));
    }
}
