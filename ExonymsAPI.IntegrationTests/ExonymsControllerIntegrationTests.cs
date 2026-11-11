using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using FluentAssertions;

namespace ExonymsAPI.IntegrationTests
{
    [TestFixture]
    public sealed class ExonymsControllerIntegrationTests
    {
        private readonly ExonymsApiFactory factory = new();
        private readonly HttpClient client;

        public ExonymsControllerIntegrationTests()
        {
            client = factory.CreateClient();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            client.Dispose();
            factory.Dispose();
        }

        [Test]
        public async Task GivenEmptyRequest_WhenGetExonyms_ThenReturnsBadRequest()
        {
            HttpResponseMessage response = await client.GetAsync("/exonyms");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GivenOnlyGeoNamesId_WhenGetExonyms_ThenReturnsOkWithExonyms()
        {
            HttpResponseMessage response = await client.GetAsync("/exonyms?geoNamesId=310350");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("GeoNamesDefault");
        }

        [Test]
        public async Task GivenOnlyWikiDataId_WhenGetExonyms_ThenReturnsOkWithExonyms()
        {
            HttpResponseMessage response = await client.GetAsync("/exonyms?wikiDataId=Q310350");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("WikiDataDefault");
        }

        [Test]
        public async Task GivenBothIds_WhenGetExonyms_ThenReturnsOkWithMergedExonyms()
        {
            HttpResponseMessage response = await client.GetAsync("/exonyms?geoNamesId=310350&wikiDataId=Q310350");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("WikiDataDefault");
            content.Should().Contain("GermanValue");
            content.Should().Contain("FrenchValue");
            content.Should().Contain("gmh");
        }

        [Test]
        public async Task GivenInvalidGeoNamesId_WhenGetExonyms_ThenReturnsOkWithEmptyResult()
        {
            HttpResponseMessage response = await client.GetAsync("/exonyms?geoNamesId=invalid");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("success");
        }
    }
}