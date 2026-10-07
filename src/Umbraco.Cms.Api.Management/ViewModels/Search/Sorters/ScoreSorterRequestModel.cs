using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Search.Sorters;

/// <summary>
/// A sorter that orders results by relevance score.
/// </summary>
public class ScoreSorterRequestModel : ISorterRequestModel
{
    /// <summary>
    /// Gets or sets the sort direction.
    /// </summary>
    public required Direction Direction { get; set; }
}
