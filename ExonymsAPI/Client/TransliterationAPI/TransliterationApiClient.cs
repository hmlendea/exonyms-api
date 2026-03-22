using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ExonymsAPI.Client.TransliterationAPI.Requests;
using ExonymsAPI.Client.TransliterationAPI.Responses;
using NuciAPI.Client;
using NuciAPI.Responses;

namespace ExonymsAPI.Client.TransliterationAPI
{
    public class TransliterationApiClient(INuciApiClient nuciApiClient) : ITransliterationApiClient
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

            GetTransliterationsRequest request = new()
            {
                Text = name,
                Language = languageCode
            };

            NuciApiResponse rawResponse = await nuciApiClient.SendRequestAsync<GetTransliterationsRequest, GetTransliterationsResponse>(
                HttpMethod.Get,
                request,
                $"Transliteration");

            if (!rawResponse.IsSuccessful)
            {
                return name;
            }

            GetTransliterationsResponse response = rawResponse as GetTransliterationsResponse;

            string transliteratedName = response.Text;

            if (string.IsNullOrWhiteSpace(transliteratedName))
            {
                return name;
            }

            return transliteratedName;
        }
    }
}
