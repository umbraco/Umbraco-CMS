namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// If/where to compress custom properties for nucache
    /// </summary>
    public enum NucachePropertyCompressionLevel
    {
        None = 0,

        /// <summary>
        /// Compress property data at the nucache SQL DB table level
        /// </summary>
        /// <remarks>
        /// Only necessary if the document in the nucache SQL DB table isn't stored as compressed bytes
        /// </remarks>
        SQLDatabase = 1,

        /// <summary>
        /// Compress property data at the nucache BTree level
        /// </summary>
        NuCacheDatabase = 2
    }
}
