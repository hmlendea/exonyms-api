using System.Threading.Tasks;

using ExonymsAPI.Service.Models;

namespace ExonymsAPI.Service
{
    /// <summary>
    /// Service for gathering exonyms from various sources.
    /// </summary>
    public interface IExonymsService
    {
        /// <summary>
        /// Gathers exonyms for a location using GeoNames and WikiData identifiers.
        /// </summary>
        /// <param name="geoNamesId">The GeoNames identifier.</param>
        /// <param name="wikiDataId">The WikiData identifier.</param>
        /// <returns>A <see cref="Location"/> containing the gathered exonyms.</returns>
        Task<Location> Gather(string geoNamesId, string wikiDataId);
    }
}
