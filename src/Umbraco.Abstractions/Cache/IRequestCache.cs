namespace Umbraco.Core.Cache
{
    public interface IRequestCache : IAppCache
    {
        bool Set(string key, object value);
        bool Remove(string key);

        /// <summary>
        /// Returns true if the request cache is available otherwise false
        /// </summary>
        bool IsAvailable { get; }
    }
}
