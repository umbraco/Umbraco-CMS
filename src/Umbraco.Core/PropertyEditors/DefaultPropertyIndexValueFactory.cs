using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Provides a default implementation for
///     <see ref="IPropertyIndexValueFactory" />, returning a single field to index containing the property value.
/// </summary>
public class DefaultPropertyIndexValueFactory : IPropertyIndexValueFactory
{
    public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published,
        IEnumerable<string> availableCultures, IDictionary<Guid, IContentType> contentTypeDictionary)
    {
        yield return new KeyValuePair<string, IEnumerable<object?>>(
            property.Alias,
            property.GetValue(culture, segment, published).Yield());
    }

    /// <inheritdoc />
    [Obsolete("Use the non-obsolete overload, scheduled for removal in v14")]
    public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture,
        string? segment, bool published, IEnumerable<string> availableCultures)
        => GetIndexValues(property, culture, segment, published, availableCultures,
            new Dictionary<Guid, IContentType>());

    [Obsolete("Use the non-obsolete overload, scheduled for removal in v14")]
    public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published)
        => GetIndexValues(property, culture, segment, published, Enumerable.Empty<string>(), new Dictionary<Guid, IContentType>());
}
