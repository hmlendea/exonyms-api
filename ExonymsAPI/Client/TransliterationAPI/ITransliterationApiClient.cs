using System.Threading.Tasks;

namespace ExonymsAPI.Client.TransliterationAPI
{
    public interface ITransliterationApiClient
    {
        Task<string> Transliterate(string languageCode, string name);
    }
}
