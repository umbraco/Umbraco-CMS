namespace Umbraco.Cms.Core.Models.ContentEditing;

public class GetAvailableCompositionsFilter
{
    public int ContentTypeId { get; set; }

    /// <summary>
    ///     This is normally an empty list but if additional property type aliases are passed in, any content types that have
    ///     these aliases will be filtered out.
    ///     This is required because in the case of creating/modifying a content type because new property types being added to
    ///     it are not yet persisted so cannot
    ///     be looked up via the db, they need to be passed in.
    /// </summary>
    public string[]? FilterPropertyTypes { get; set; }

    /// <summary>
    ///     This is normally an empty list but if additional content type aliases are passed in, any content types containing
    ///     those aliases will be filtered out
    ///     along with any content types that have matching property types that are included in the filtered content types
    /// </summary>
    public string[]? FilterContentTypes { get; set; }

    /// <summary>
    ///     Wether the content type is currently marked as an element type
    /// </summary>
    public bool IsElement { get; set; }
}
