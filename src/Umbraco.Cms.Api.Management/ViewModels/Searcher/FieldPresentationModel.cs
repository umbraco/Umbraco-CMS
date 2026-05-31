namespace Umbraco.Cms.Api.Management.ViewModels.Searcher;

/// <summary>
/// Defines the data structure used to present information about a searcher field in the API.
/// </summary>
public class FieldPresentationModel
{
    /// <summary>
    /// Gets the name of the field.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets the collection of values associated with this field.
    /// </summary>
    public IEnumerable<string> Values { get; init; } = null!;
}
