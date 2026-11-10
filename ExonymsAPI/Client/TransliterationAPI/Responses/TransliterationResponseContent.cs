using System.Text.Json.Serialization;
using NuciAPI.Responses;

namespace ExonymsAPI.Client.TransliterationAPI.Responses
{
    public sealed class TransliterationResponseContent : NuciApiResponseContent
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}