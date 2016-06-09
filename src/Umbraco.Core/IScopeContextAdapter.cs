namespace Umbraco.Core
{
    // fixme - kill, eventually, once we have accessors sorted out
    internal interface IScopeContextAdapter
    {
        // ok to get a non-existing key, returns null

        object Get(string key);
        void Set(string key, object value);
        void Clear(string key);
    }
}