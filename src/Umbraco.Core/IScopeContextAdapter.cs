namespace Umbraco.Core
{
    internal interface IScopeContextAdapter
    {
        // ok to get a non-existing key, returns null

        object Get(string key);
        void Set(string key, object value);
        void Clear(string key);
    }
}