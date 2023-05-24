using System.Threading.Tasks;

namespace ExonymsAPI.Service
{
    public interface IExonymsService
    {
        Task<string> Gather(string wikiDataId);
    }
}
