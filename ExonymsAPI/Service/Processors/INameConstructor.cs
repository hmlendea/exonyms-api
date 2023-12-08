namespace ExonymsAPI.Service.Processors
{
    public interface INameConstructor
    {
        string Construct(string baseName, string language);
    }
}
