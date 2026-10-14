using System.Threading.Tasks;

namespace ExonymsAPI.Client.TransliterationAPI
{
    /// <summary>
    /// Client for transliterating names between scripts.
    /// </summary>
    public interface ITransliterationApiClient
    {
        /// <summary>
        /// Transliterates a name from its native script to Latin script.
        /// </summary>
        /// <param name="languageCode">The language code of the name.</param>
        /// <param name="name">The name to transliterate.</param>
        /// <returns>The transliterated name, or the original name if transliteration is not supported or fails.</returns>
        Task<string> Transliterate(string languageCode, string name);
    }
}
