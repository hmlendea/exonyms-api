namespace ExonymsAPI.Service.Normalisers
{
    public interface INameNormaliser
    {
        string Normalise(string languageCode, string name);
    }
}
