using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ExonymsAPI.Configuration;

namespace ExonymsAPI.Service.Normalisers
{
    public class NameTransliterator : INameTransliterator
    {
        readonly TransliterationSettings transliterationSettings;
        readonly HttpClient client;

        IList<string> languageCodesToTransliterate;

        public NameTransliterator(TransliterationSettings transliterationSettings)
        {
            this.transliterationSettings = transliterationSettings;
            client = new HttpClient();

            languageCodesToTransliterate = new List<string>
            {
                "ab", // Abkhaz
                "ady", // Adyghe
                "ar", // Arabic
                "ary", // Maghrebi Arabic
                "arz", // Egyptian Arabic
                "ba", // Bashkir
                "be", // Belarussian
                "bg", // Bulgarian
                "bn", // Bengali
                "cop", // Coptic
                "cu", // Old Church Slavonic
                "cv", // Chuvash
                "el", // Greek
                "grc", // Ancient Greek
                "grc-dor", // Ancient Greek - Doric
                "gu", // Gujarati
                "he", // Hebrew
                "hi", // Hindi
                "hy", // Armenian
                "hyw", // Western Armenian
                "iu", // Inuttitut
                "ja", // Japanese
                "ka", // Georgian
                "kk", // Kazakh
                "kn", // Kannada
                "ko", // Korean
                "ky", // Kyrgyz
                "mk", // Macedonian Slavic
                "ml", // Malayalam
                "mn", // Mongol
                "mr", // Marathi
                "os", // Ossetic
                "ru", // Russian
                "sa", // Sanskrit
                "sh", // SerboCroatian
                "si", // Sinhala
                "sr", // Serbian
                "sr-ec", // Serbian Cyrillic
                "ta", // Tamil
                "te", // Telugu
                "th", // Thai
                "udm", // Udmurt
                "uk", // Ukrainian
                "zh", // Chinese
                "zh-hans", // Simplified Chinese
            };
        }

        public async Task<string> Transliterate(string languageCode, string name)
        {
            if (!languageCodesToTransliterate.Contains(languageCode))
            {
                return name;
            }

            HttpResponseMessage response = await client.GetAsync($"{transliterationSettings.TransliterationApiBaseUrl}/Transliteration?text={name}&language={languageCode}");

            if (!response.IsSuccessStatusCode)
            {
                return name;
            }

            string transliteratedName = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(transliteratedName))
            {
                return name;
            }

            return transliteratedName;
        }
    }
}
