using System.Threading.Tasks;

using ExonymsAPI.Service.Models;

namespace ExonymsAPI.Service.Gatherers
{
    /// <summary>
    /// Gathers exonyms from WikiData.
    /// </summary>
    public interface IWikiDataGatherer
    {
        /// <summary>
        /// Gathers exonyms for a location using a WikiData identifier.
        /// </summary>
        /// <param name="wikiDataId">The WikiData identifier.</param>
        /// <returns>A <see cref="Location"/> containing the gathered exonyms.</returns>
        Task<Location> Gather(string wikiDataId);
    }
}
