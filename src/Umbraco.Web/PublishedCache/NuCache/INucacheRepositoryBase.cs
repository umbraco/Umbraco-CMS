using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache
{
    /// <summary>
    /// Repository For Nucache
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    ///  /// <remarks>Ensure the repository is responsible for queries. The underlying storage may support more efficent queries</remarks>
    public interface INucacheRepositoryBase<TKey, TValue> : ITransactableDictionary<TKey, TValue>
    {
        /// <summary>
        /// Get All by default sort order
        /// </summary>
        /// <returns></returns>
        ICollection<TValue> GetAllSorted();
    }
}
