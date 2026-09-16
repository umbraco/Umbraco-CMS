using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Cms.Search.Provider.Examine.Services;

/// <summary>
/// Marker interface for <see cref="IIndexer"/>, allowing explicit index registrations against the Examine-based indexer.
/// </summary>
public interface IExamineIndexer : IIndexer
{
}
