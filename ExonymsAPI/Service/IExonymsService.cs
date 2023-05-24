using System.Threading.Tasks;

using ExonymsAPI.Service.Models;

namespace ExonymsAPI.Service
{
    public interface IExonymsService
    {
        Task<Location> Gather(string wikiDataId);
    }
}
