namespace ExonymsAPI.Service.Processors
{
    public interface INameNormaliser
    {
        string Normalise(string languageCode, string name);
    }
}
