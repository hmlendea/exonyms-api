using NuciAPI.Responses;

namespace ExonymsAPI.Client.TransliterationAPI.Responses
{
    public class GetTransliterationsResponse : NuciApiSuccessResponse
    {
        public string Text { get; set; }
    }
}
