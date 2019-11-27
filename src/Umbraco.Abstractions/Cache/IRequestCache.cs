namespace Umbraco.Core.Cache
{
    public interface IRequestCache : IAppCache
    {
        bool Set(string key, object value);
        bool Remove(string key);
    }
}
