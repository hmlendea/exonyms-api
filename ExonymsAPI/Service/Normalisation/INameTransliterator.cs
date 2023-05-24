using System.Threading.Tasks;

namespace ExonymsAPI.Service.Normalisers
{
    public interface INameTransliterator
    {
        Task<string> Transliterate(string languageCode, string name);
    }
}
