namespace ExonymsAPI.Service.Processors
{
    /// <summary>
    /// Constructs names for languages based on a base name.
    /// </summary>
    public interface INameConstructor
    {
        /// <summary>
        /// Constructs a name for a specific language based on a base name.
        /// </summary>
        /// <param name="baseName">The base name to construct from.</param>
        /// <param name="language">The target language code.</param>
        /// <returns>The constructed name.</returns>
        string Construct(string baseName, string language);
    }
}
