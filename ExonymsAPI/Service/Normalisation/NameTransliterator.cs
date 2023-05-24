using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExonymsAPI.Service.Normalisers
{
    public class NameTransliterator : INameTransliterator
    {
        HttpClient client;

        public NameTransliterator()
        {
            client = new HttpClient();
        }

        public async Task<string> Transliterate(string languageCode, string name)
        {
            HttpResponseMessage response = await client.GetAsync($"http://hmlendea-translit.duckdns.org:9584/Transliteration?text={name}&language={languageCode}");

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
