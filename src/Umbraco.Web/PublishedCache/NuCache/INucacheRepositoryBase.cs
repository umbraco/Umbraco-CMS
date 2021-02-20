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
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface INucacheRepositoryBase<TKey, TValue> : ITransactableDictionary<TKey, TValue>
    {
        /// <summary>
        /// Get All by default sort order
        /// </summary>
        /// <returns></returns>
        ICollection<TValue> GetAllSorted();
    }
}
