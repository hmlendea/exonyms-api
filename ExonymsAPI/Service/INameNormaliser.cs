namespace ExonymsAPI.Service
{
    public interface INameNormaliser
    {
        string NormaliseName(string languageCode, string name);
    }
}
