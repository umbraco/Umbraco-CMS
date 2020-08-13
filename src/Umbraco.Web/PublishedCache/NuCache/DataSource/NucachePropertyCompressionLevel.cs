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
        /// Idea being we only compress this once.
        /// All the records in cmsContentNu need to be rebuilt when this gets enabled.
        /// Good option as then we don't use up memory / cpu to compress at boot.
        /// </remarks>
        SQLDatabase = 1,

        /// <summary>
        /// Compress property data at the nucache BTree level
        /// </summary>
        /// <remarks>
        /// Compress the property when writing to nucache bplustree after reading from the database.
        /// Idea being we compress this at rebuild / boot.
        /// This option supports older items not being compressed already, at the expense of doing this compression at boot.
        /// But it also means you can easily switch between None and NuCacheDatabase if performance is worse.
        /// </remarks>
        NuCacheDatabase = 2
    }
}
