using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Search.Sorters;

/// <summary>
/// A sorter that orders results by a text field.
/// </summary>
public class TextSorterRequestModel : ISorterRequestModel
{
    /// <summary>
    /// Gets or sets the name of the field to sort by.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the sort direction.
    /// </summary>
    public required Direction Direction { get; set; }
}
