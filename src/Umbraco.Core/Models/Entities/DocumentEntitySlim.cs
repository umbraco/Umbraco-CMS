namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Implements <see cref="IDocumentEntitySlim" />.
/// </summary>
public class DocumentEntitySlim : ContentEntitySlim, IDocumentEntitySlim
{
    private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();

    private IReadOnlyDictionary<string, string>? _cultureNames;
    private IEnumerable<string>? _editedCultures;
    private IEnumerable<string>? _publishedCultures;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> CultureNames
    {
        get => _cultureNames ?? Empty;
        set => _cultureNames = value;
    }

    /// <inheritdoc />
    public IEnumerable<string> PublishedCultures
    {
        get => _publishedCultures ?? Enumerable.Empty<string>();
        set => _publishedCultures = value;
    }

    /// <inheritdoc />
    public IEnumerable<string> EditedCultures
    {
        get => _editedCultures ?? Enumerable.Empty<string>();
        set => _editedCultures = value;
    }

    public ContentVariation Variations { get; set; }

    /// <inheritdoc />
    public bool Published { get; set; }

    /// <inheritdoc />
    public bool Edited { get; set; }
}
