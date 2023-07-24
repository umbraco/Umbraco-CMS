namespace Umbraco.Search;

/// <summary>
///     A Marker interface for defining an Umbraco indexer
/// </summary>
public interface IUmbracoIndex<T> : IUmbracoIndex
{
    void IndexItems(T[] members);
}

public interface IUmbracoIndex : IDisposable
{
    string Name { get; }
    Action<object?, EventArgs>? IndexOperationComplete { get; set; }
    long GetDocumentCount();
    bool Exists();
    void Create();
    IEnumerable<string> GetFieldNames();
    void RemoveFromIndex(IEnumerable<string> ids);
}
