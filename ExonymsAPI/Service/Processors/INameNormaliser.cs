namespace ExonymsAPI.Service.Processors
{
    /// <summary>
    /// Normalises names by removing unwanted words and patterns.
    /// </summary>
    public interface INameNormaliser
    {
        /// <summary>
        /// Normalises a name for a specific language.
        /// </summary>
        /// <param name="languageCode">The language code.</param>
        /// <param name="name">The name to normalise.</param>
        /// <returns>The normalised name.</returns>
        string Normalise(string languageCode, string name);
    }
}
