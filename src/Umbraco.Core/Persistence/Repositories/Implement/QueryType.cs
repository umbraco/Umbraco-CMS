namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Specifies the type of base query.
    /// </summary>
    internal enum QueryType
    {
        /// <summary>
        /// Get one single complete item.
        /// </summary>
        Single,

        /// <summary>
        /// Get many complete items.
        /// </summary>
        Many,

        /// <summary>
        /// Get item identifiers only.
        /// </summary>
        Ids,

        /// <summary>
        /// Count items.
        /// </summary>
        Count
    }
}
