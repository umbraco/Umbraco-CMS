namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// If/where to decompress custom properties for nucache
    /// </summary>
    public enum NucachePropertyDecompressionLevel
    {
        NotCompressed = 0,
        Immediate = 1,
        Lazy = 2
    }
}
