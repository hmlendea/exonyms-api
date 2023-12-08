using System.Threading.Tasks;

namespace ExonymsAPI.Service.Processors
{
    public interface INameTransliterator
    {
        Task<string> Transliterate(string languageCode, string name);
    }
}
