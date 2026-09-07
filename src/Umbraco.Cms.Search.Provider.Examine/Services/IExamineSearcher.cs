using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Cms.Search.Provider.Examine.Services;

/// <summary>
/// Marker interface for <see cref="ISearcher"/>, allowing explicit index registrations against the Examine-based searcher.
/// </summary>
public interface IExamineSearcher : ISearcher
{
}
