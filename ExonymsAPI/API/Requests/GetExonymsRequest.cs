using System.Text.Json.Serialization;
using NuciAPI.Requests;
using NuciAPI.Responses;

namespace ExonymsAPI.API.Requests
{
    public sealed class GetExonymsRequest : NuciApiRequest
    {
        [JsonPropertyName("geoNamesId")]
        public string GeoNamesId { get; set; }

        [JsonPropertyName("wikiDataId")]
        public string WikiDataId { get; set; }
    }
}
