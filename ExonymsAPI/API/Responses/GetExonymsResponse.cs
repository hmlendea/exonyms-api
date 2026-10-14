using System.Collections.Generic;
using System.Text.Json.Serialization;
using ExonymsAPI.Service.Models;
using NuciAPI.Responses;

namespace ExonymsAPI.API.Responses
{
    public sealed class GetExonymsResponse : NuciApiSuccessResponse
    {
        [JsonPropertyName("defaultName")]
        public string DefaultName { get; set; }

        [JsonPropertyName("names")]
        public IDictionary<string, Name> Names { get; set; } = new Dictionary<string, Name>();

        [JsonPropertyName("count")]
        public int Count => Names.Count;
    }
}
