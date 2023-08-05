using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Search.Diagnostics;

namespace Umbraco.Search;

/// <summary>
///     A Marker interface for defining an Umbraco indexer
/// </summary>
public interface IUmbracoIndex<T> : IUmbracoIndex where T : IUmbracoEntity
{
    void IndexItems(T[] members);
}

public interface IUmbracoIndex : IDisposable
{
    string Name { get; }
    Action<object?, EventArgs>? IndexOperationComplete { get; set; }
    ISearchEngine? SearchEngine { get; }
    long GetDocumentCount();
    bool Exists();
    void Create();
    IEnumerable<string> GetFieldNames();
    void RemoveFromIndex(IEnumerable<string> ids);

}
