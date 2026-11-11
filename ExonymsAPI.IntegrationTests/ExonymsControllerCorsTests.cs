using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

namespace ExonymsAPI.IntegrationTests
{
    [TestFixture]
    public sealed class ExonymsControllerCorsTests
    {
        private static string AccessControlAllowMethodsName => "Access-Control-Allow-Methods";

        private static string AccessControlAllowOriginName => "Access-Control-Allow-Origin";

        private static string AccessControlRequestMethodName => "Access-Control-Request-Method";

        private static string OriginHeaderName => "Origin";

        private readonly ExonymsApiFactory factory = new();
        private readonly HttpClient client;

        public ExonymsControllerCorsTests()
        {
            client = factory.CreateClient();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            client.Dispose();
            factory.Dispose();
        }

        [TestCase("https://hmlendea.github.io")]
        [TestCase("http://localhost:5000")]
        [TestCase("https://localhost:5001")]
        [TestCase("http://localhost:7000")]
        [TestCase("https://localhost:7001")]
        [TestCase("http://localhost:8080")]
        [TestCase("http://localhost:8081")]
        public async Task GivenAnAllowedOrigin_WhenGettingExonyms_ThenTheOriginIsPermitted(
            string origin)
        {
            using HttpRequestMessage request = CreateExonymsRequest(origin);

            using HttpResponseMessage response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Headers.Contains(AccessControlAllowOriginName).Should().BeTrue();
            response.Headers.GetValues(AccessControlAllowOriginName).Should().Equal(origin);
        }

        [TestCase("http://localhost:5001")]
        [TestCase("https://localhost:5000")]
        [TestCase("https://dummy-domain.com")]
        [TestCase("https://test.url.ro")]
        public async Task GivenADisallowedOrigin_WhenGettingExonyms_ThenTheOriginIsNotPermitted(
            string origin)
        {
            using HttpRequestMessage request = CreateExonymsRequest(origin);

            using HttpResponseMessage response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Headers.Contains(AccessControlAllowOriginName).Should().BeFalse();
        }

        [TestCase("https://hmlendea.github.io")]
        [TestCase("http://localhost:5000")]
        [TestCase("https://localhost:5001")]
        public async Task GivenAnAllowedOrigin_WhenSendingAPreflightRequest_ThenCorsHeadersAreReturned(
            string origin)
        {
            using HttpRequestMessage request = CreatePreflightRequest(origin, HttpMethod.Get.Method);

            using HttpResponseMessage response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            response.Headers.GetValues(AccessControlAllowOriginName).Should().Equal(origin);
            response.Headers.GetValues(AccessControlAllowMethodsName).Should().Contain(HttpMethod.Get.Method);
        }

        [TestCase("http://localhost:5001")]
        [TestCase("https://dummy-domain.com")]
        public async Task GivenADisallowedOrigin_WhenSendingAPreflightRequest_ThenNoCorsHeadersAreReturned(
            string origin)
        {
            using HttpRequestMessage request = CreatePreflightRequest(origin, HttpMethod.Get.Method);

            using HttpResponseMessage response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            response.Headers.Contains(AccessControlAllowOriginName).Should().BeFalse();
            response.Headers.Contains(AccessControlAllowMethodsName).Should().BeFalse();
        }

        private static HttpRequestMessage CreateExonymsRequest(string origin)
        {
            HttpRequestMessage request = new(HttpMethod.Get, "/exonyms?wikiDataId=Q310350");
            request.Headers.TryAddWithoutValidation(OriginHeaderName, origin);

            return request;
        }

        private static HttpRequestMessage CreatePreflightRequest(string origin, string requestedMethod)
        {
            HttpRequestMessage request = new(HttpMethod.Options, "/exonyms");
            request.Headers.TryAddWithoutValidation(OriginHeaderName, origin);
            request.Headers.TryAddWithoutValidation(AccessControlRequestMethodName, requestedMethod);

            return request;
        }
    }
}
