using System.Threading.Tasks;

using ExonymsAPI.Service.Models;

namespace ExonymsAPI.Service.Gatherers
{
    public interface IWikiDataGatherer
    {
        Task<Location> Gather(string wikiDataId);
    }
}
