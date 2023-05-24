using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ExonymsAPI.Service.Gatherers;
using ExonymsAPI.Service.Models;

namespace ExonymsAPI.Service
{
    public class ExonymsService : IExonymsService
    {
        IGeoNamesGatherer geoNamesGatherer;
        IWikiDataGatherer wikiDataGatherer;

        public ExonymsService(
            IGeoNamesGatherer geoNamesGatherer,
            IWikiDataGatherer wikiDataGatherer)
        {
            this.geoNamesGatherer = geoNamesGatherer;
            this.wikiDataGatherer = wikiDataGatherer;
        }

        public async Task<Location> Gather(string geoNamesId, string wikiDataId)
        {
            IList<Location> gatheredLocations = new List<Location>();

            if (!string.IsNullOrWhiteSpace(wikiDataId))
            {
                gatheredLocations.Add(await wikiDataGatherer.Gather(wikiDataId));
            }

            if (!string.IsNullOrWhiteSpace(geoNamesId))
            {
                gatheredLocations.Add(await geoNamesGatherer.Gather(geoNamesId));
            }

            Location location = new Location();

            foreach (Location gatheredLocation in gatheredLocations)
            {
                if (string.IsNullOrWhiteSpace(location.DefaultName))
                {
                    location.DefaultName = gatheredLocation.DefaultName;
                }

                foreach (var name in gatheredLocation.Names.Where(x => !location.Names.ContainsKey(x.Key)))
                {
                    location.Names.TryAdd(name.Key, name.Value);
                }
            }

            location.Names = location.Names
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);

            return location;
        }
    }
}
