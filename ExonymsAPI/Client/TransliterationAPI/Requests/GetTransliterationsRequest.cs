using NuciAPI.Requests;

namespace ExonymsAPI.Client.TransliterationAPI.Requests
{
    public class GetTransliterationsRequest : NuciApiRequest
    {
        public string Text { get; set; }

        public string Language { get; set; }
    }
}
