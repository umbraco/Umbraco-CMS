using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Search.Sorters;

/// <summary>
/// A sorter that orders results by an integer field.
/// </summary>
public class IntegerSorterRequestModel : ISorterRequestModel
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
