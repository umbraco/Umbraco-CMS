namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// If/where to compress custom properties for nucache
    /// </summary>
    public enum NucachePropertyCompressionLevel
    {
        None = 0,

        /// <summary>
        /// Compress property data at the nucache BTree level
        /// </summary>
        NuCacheDatabase = 2
    }
}
