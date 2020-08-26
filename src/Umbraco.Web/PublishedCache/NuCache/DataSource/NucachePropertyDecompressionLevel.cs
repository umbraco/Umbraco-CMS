namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// If/where to decompress custom properties for nucache
    /// </summary>
    public enum NucachePropertyDecompressionLevel
    {
        NotCompressed = 0,

        // TODO: I'm unsure if this will ever be necessary, lazy seems good and deserialization would only occur once
        Immediate = 1,

        Lazy = 2
    }
}
