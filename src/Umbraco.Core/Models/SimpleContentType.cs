using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Implements <see cref="ISimpleContentType" />.
/// </summary>
public class SimpleContentType : ISimpleContentType
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SimpleContentType" /> class.
    /// </summary>
    public SimpleContentType(IContentType contentType)
        : this((IContentTypeBase)contentType) =>
        DefaultTemplate = contentType.DefaultTemplate;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SimpleContentType" /> class.
    /// </summary>
    public SimpleContentType(IMediaType mediaType)
        : this((IContentTypeBase)mediaType)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SimpleContentType" /> class.
    /// </summary>
    public SimpleContentType(IMemberType memberType)
        : this((IContentTypeBase)memberType)
    {
    }

    private SimpleContentType(IContentTypeBase contentType)
    {
        if (contentType == null)
        {
            throw new ArgumentNullException(nameof(contentType));
        }

        Id = contentType.Id;
        Key = contentType.Key;
        Alias = contentType.Alias;
        Variations = contentType.Variations;
        Icon = contentType.Icon;
        IsContainer = contentType.IsContainer;
        Name = contentType.Name;
        AllowedAsRoot = contentType.AllowedAsRoot;
        IsElement = contentType.IsElement;
    }

    public string Alias { get; }

    public int Id { get; }

    public Guid Key { get; }

    /// <inheritdoc />
    public ITemplate? DefaultTemplate { get; }

    public ContentVariation Variations { get; }

    public string? Icon { get; }

    public bool IsContainer { get; }

    public string? Name { get; }

    public bool AllowedAsRoot { get; }

    public bool IsElement { get; }

    public bool SupportsPropertyVariation(string? culture, string segment, bool wildcards = false) =>

        // non-exact validation: can accept a 'null' culture if the property type varies
        //  by culture, and likewise for segment
        // wildcard validation: can accept a '*' culture or segment
        Variations.ValidateVariation(culture, segment, false, wildcards, false);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((SimpleContentType)obj);
    }

    protected bool Equals(SimpleContentType other) => string.Equals(Alias, other.Alias) && Id == other.Id;

    public override int GetHashCode()
    {
        unchecked
        {
            return ((Alias != null ? Alias.GetHashCode() : 0) * 397) ^ Id;
        }
    }
}
