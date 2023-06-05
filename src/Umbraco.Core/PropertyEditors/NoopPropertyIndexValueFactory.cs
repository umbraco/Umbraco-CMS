using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Property Index Valye Factory that do not index anything.
/// </summary>
public class NoopPropertyIndexValueFactory : IPropertyIndexValueFactory
{
    /// <inheritdoc />
    public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published) => Array.Empty<KeyValuePair<string, IEnumerable<object?>>>();
}
