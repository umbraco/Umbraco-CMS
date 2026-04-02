namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Provides a read-only adapter for <see cref="IContentBase" /> instances.
/// </summary>
/// <remarks>
///     This struct wraps an <see cref="IContentBase" /> instance and exposes its properties
///     through the <see cref="IReadOnlyContentBase" /> interface without allowing modifications.
/// </remarks>
public struct ReadOnlyContentBaseAdapter : IReadOnlyContentBase
{
    private readonly IContentBase _content;

    private ReadOnlyContentBaseAdapter(IContentBase content) =>
        _content = content ?? throw new ArgumentNullException(nameof(content));

    /// <inheritdoc />
    public int Id => _content.Id;

    /// <summary>
    ///     Creates a new <see cref="ReadOnlyContentBaseAdapter" /> instance from an <see cref="IContentBase" />.
    /// </summary>
    /// <param name="content">The content base to wrap.</param>
    /// <returns>A read-only adapter for the specified content.</returns>
    public static ReadOnlyContentBaseAdapter Create(IContentBase content) => new(content);

    /// <inheritdoc />
    public Guid Key => _content.Key;

    /// <inheritdoc />
    public DateTime CreateDate => _content.CreateDate;

    /// <inheritdoc />
    public DateTime UpdateDate => _content.UpdateDate;

    /// <inheritdoc />
    public string? Name => _content.Name;

    /// <inheritdoc />
    public int CreatorId => _content.CreatorId;

    /// <inheritdoc />
    public int ParentId => _content.ParentId;

    /// <inheritdoc />
    public int Level => _content.Level;

    /// <inheritdoc />
    public string Path => _content.Path;

    /// <inheritdoc />
    public int SortOrder => _content.SortOrder;

    /// <inheritdoc />
    public int ContentTypeId => _content.ContentTypeId;

    /// <inheritdoc />
    public int WriterId => _content.WriterId;

    /// <inheritdoc />
    public int VersionId => _content.VersionId;
}
