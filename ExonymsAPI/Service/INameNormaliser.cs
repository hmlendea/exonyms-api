namespace ExonymsAPI.Service
{
    public interface INameNormaliser
    {
        string Normalise(string languageCode, string name);
    }
}
