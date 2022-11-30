using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a search result by entity type
/// </summary>
[DataContract(Name = "searchResult", Namespace = "")]
public class TreeSearchResult
{
    [DataMember(Name = "appAlias")]
    public string? AppAlias { get; set; }

    [DataMember(Name = "treeAlias")]
    public string? TreeAlias { get; set; }

    /// <summary>
    ///     This is optional but if specified should be the name of an angular service to format the search result.
    /// </summary>
    [DataMember(Name = "jsSvc")]
    public string? JsFormatterService { get; set; }

    /// <summary>
    ///     This is optional but if specified should be the name of a method on the jsSvc angular service to use, if not
    ///     specified than it will expect the method to be called `format(searchResult, appAlias, treeAlias)`
    /// </summary>
    [DataMember(Name = "jsMethod")]
    public string? JsFormatterMethod { get; set; }

    [DataMember(Name = "results")]
    public IEnumerable<SearchResultEntity>? Results { get; set; }
}
