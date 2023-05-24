using System;
using System.Threading.Tasks;

using ExonymsAPI.Service.Gatherers;
using ExonymsAPI.Service.Models;

namespace ExonymsAPI.Service
{
    public class ExonymsService : IExonymsService
    {
        IWikiDataGatherer wikiDataGatherer;

        public ExonymsService(
            IWikiDataGatherer wikiDataGatherer)
        {
            this.wikiDataGatherer = wikiDataGatherer;
        }

        public async Task<Location> Gather(string wikiDataId)
        {
            Location location = await this.wikiDataGatherer.Gather(wikiDataId);

            return location;
        }
    }
}
