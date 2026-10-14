using System.Threading.Tasks;

using ExonymsAPI.Service.Models;

namespace ExonymsAPI.Service.Gatherers
{
    /// <summary>
    /// Gathers exonyms from GeoNames.
    /// </summary>
    public interface IGeoNamesGatherer
    {
        /// <summary>
        /// Gathers exonyms for a location using a GeoNames identifier.
        /// </summary>
        /// <param name="geoNamesId">The GeoNames identifier.</param>
        /// <returns>A <see cref="Location"/> containing the gathered exonyms.</returns>
        Task<Location> Gather(string geoNamesId);
    }
}
