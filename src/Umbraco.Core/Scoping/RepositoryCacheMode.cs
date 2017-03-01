namespace Umbraco.Core.Scoping
{
    public enum RepositoryCacheMode
    {
        // ?
        Unspecified = 0,

        // the default, full L2 cache
        Default = 1,

        // a scoped cache
        // reads from and writes to a local cache
        // clears the global cache on completion
        Scoped = 2
    }
}
