using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Provides a default implementation for
///     <see ref="IPropertyIndexValueFactory">, returning a single field to index containing the property value.
/// </summary>
public class DefaultPropertyIndexValueFactory : IPropertyIndexValueFactory
{
    /// <inheritdoc />
    public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published)
    {
        yield return new KeyValuePair<string, IEnumerable<object?>>(
            property.Alias,
            property.GetValue(culture, segment, published).Yield());
    }
}
