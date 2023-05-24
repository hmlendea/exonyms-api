namespace ExonymsAPI.Service.Gatherers
{
    public interface IWikiDataGatherer
    {
        string Gather(string wikiDataId);
    }
}
