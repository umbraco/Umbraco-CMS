namespace Umbraco.Core.Persistence.Repositories
{
    internal enum BaseQueryType
    {
        /// <summary>
        /// A query to return all information for a single item
        /// </summary>
        /// <remarks>
        /// In some cases this will be the same as <see cref="FullMultiple"/>
        /// </remarks>
        FullSingle,

        /// <summary>
        /// A query to return all information for multiple items
        /// </summary>
        /// <remarks>
        /// In some cases this will be the same as <see cref="FullSingle"/>
        /// </remarks>
        FullMultiple,

        /// <summary>
        /// A query to return the ids for items
        /// </summary>
        Ids,

        /// <summary>
        /// A query to return the count for items 
        /// </summary>
        Count
    }
}