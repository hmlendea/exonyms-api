using System;
using System.Threading.Tasks;

using ExonymsAPI.Service.Gatherers;

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

        public async Task<string> Gather(string wikiDataId)
        {
            string name = wikiDataGatherer.Gather(wikiDataId);

            return name;
        }
    }
}
