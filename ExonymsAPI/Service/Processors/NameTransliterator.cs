using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ExonymsAPI.Configuration;

namespace ExonymsAPI.Service.Processors
{
    public class NameTransliterator(TransliterationSettings transliterationSettings) : INameTransliterator
    {
        readonly HttpClient client = new();

        readonly IList<string> languageCodesToTransliterate =
            [
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
                "sr-ec", // Serbian Cyrillic
                "sr", // Serbian
                "ta", // Tamil
                "te", // Telugu
                "tg-cyrl", // Tajik Cyrillic
                "tg", // Tajik
                "th", // Thai
                "tt-cyrl", // Tatar Cyrillic
                "tt", // Tatar
                "udm", // Udmurt
                "uk", // Ukrainian
                "zh", // Chinese
                "zh-hans", // Simplified Chinese
            ];

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
