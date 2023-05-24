using System.Threading.Tasks;

using ExonymsAPI.Service.Models;

namespace ExonymsAPI.Service.Gatherers
{
    public interface IGeoNamesGatherer
    {
        Task<Location> Gather(string geoNamesId);
    }
}
