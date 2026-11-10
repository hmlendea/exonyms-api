using NuciAPI.Responses;

namespace ExonymsAPI.Client.TransliterationAPI.Responses
{
    public sealed class TransliterationResponseContent : NuciApiResponseContent
    {
        public string Text { get; set; }
    }
}