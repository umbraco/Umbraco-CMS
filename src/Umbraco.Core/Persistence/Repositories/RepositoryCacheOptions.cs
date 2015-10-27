namespace Umbraco.Core.Persistence.Repositories
{
    internal class RepositoryCacheOptions
    {
        /// <summary>
        /// Constructor sets defaults
        /// </summary>
        public RepositoryCacheOptions()
        {
            GetAllCacheValidateCount = true;
            GetAllCacheAllowZeroCount = false;
            GetAllCacheThresholdLimit = 100;
        }

        /// <summary>
        /// True/false as to validate the total item count when all items are returned from cache, the default is true but this
        /// means that a db lookup will occur - though that lookup will probably be significantly less expensive than the normal 
        /// GetAll method. 
        /// </summary>
        /// <remarks>
        /// setting this to return false will improve performance of GetAll cache with no params but should only be used
        /// for specific circumstances
        /// </remarks>
        public bool GetAllCacheValidateCount { get; set; }

        /// <summary>
        /// True if the GetAll method will cache that there are zero results so that the db is not hit when there are no results found
        /// </summary>
        public bool GetAllCacheAllowZeroCount { get; set; }

        /// <summary>
        /// The threshold entity count for which the GetAll method will cache entities
        /// </summary>
        public int GetAllCacheThresholdLimit { get; set; }
    }
}