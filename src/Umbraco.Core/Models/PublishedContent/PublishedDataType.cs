using System.Diagnostics;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Represents a published data type.
/// </summary>
/// <remarks>
///     <para>
///         Instances of the <see cref="PublishedDataType" /> class are immutable, ie
///         if the data type changes, then a new class needs to be created.
///     </para>
///     <para>These instances should be created by an <see cref="IPublishedContentTypeFactory" />.</para>
/// </remarks>
[DebuggerDisplay("{EditorAlias}")]
public class PublishedDataType
{
    // TODO KJA: constructor breakage
    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedDataType" /> class.
    /// </summary>
    public PublishedDataType(int id, Guid key, string editorAlias, string? editorUiAlias)
    {
        Id = id;
        Key = key;
        EditorAlias = editorAlias;
        EditorUiAlias = editorUiAlias ?? editorAlias;
    }

    /// <summary>
    ///     Gets the datatype identifier.
    /// </summary>
    public int Id { get; }

    /// <summary>
    ///     Gets the datatype unique identifier.
    /// </summary>
    public Guid Key { get; }

    /// <summary>
    ///     Gets the data type editor alias.
    /// </summary>
    public string EditorAlias { get; }

    /// <summary>
    ///     Gets the data type editor UI alias.
    /// </summary>
    public string EditorUiAlias { get; }
}
