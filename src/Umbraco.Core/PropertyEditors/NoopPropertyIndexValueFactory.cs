using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Property Index Valye Factory that do not index anything.
/// </summary>
public class NoopPropertyIndexValueFactory : IPropertyIndexValueFactory
{
    /// <inheritdoc />
    public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published,
        IEnumerable<string> availableCultures, IDictionary<Guid, IContentType> contentTypeDictionary)
    => Array.Empty<KeyValuePair<string, IEnumerable<object?>>>();


    [Obsolete("Use the overload with the availableCultures parameter instead, scheduled for removal in v14")]
    public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published, IEnumerable<string> availableCultures) => Array.Empty<KeyValuePair<string, IEnumerable<object?>>>();

    [Obsolete("Use the overload with the availableCultures parameter instead, scheduled for removal in v14")]
    public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published)
        => GetIndexValues(property, culture, segment, published);
}
