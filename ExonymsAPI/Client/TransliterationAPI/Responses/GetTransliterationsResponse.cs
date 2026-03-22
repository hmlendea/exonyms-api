using System.Text.Json.Serialization;
using NuciAPI.Responses;

namespace ExonymsAPI.Client.TransliterationAPI.Responses
{
    public class GetTransliterationsResponse : NuciApiSuccessResponse
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
