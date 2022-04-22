using System.Collections.Generic;

namespace Umbraco.Cms.Core.Cache
{
    public interface IRequestCache : IAppCache, IEnumerable<KeyValuePair<string, object?>>
    {
        bool Set(string key, object? value);
        bool Remove(string key);

        /// <summary>
        /// Returns true if the request cache is available otherwise false
        /// </summary>
        bool IsAvailable { get; }
    }
}
