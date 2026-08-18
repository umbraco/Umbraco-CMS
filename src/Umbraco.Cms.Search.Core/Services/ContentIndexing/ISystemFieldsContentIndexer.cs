namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Marker interface to register the default implementation of the content indexer for system fields (e.g. Id, Name, Path).
/// </summary>
public interface ISystemFieldsContentIndexer : IContentIndexer
{
}
