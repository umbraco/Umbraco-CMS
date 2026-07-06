using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

public interface IPropertyValueHandler : IDiscoverable
{
    /// <summary>
    /// Determines whether the property value handler can handle a concrete property.
    /// </summary>
    /// <param name="propertyEditorAlias">The property editor alias of the property.</param>
    /// <returns>True if the property can be handled, false otherwise.</returns>
    bool CanHandle(string propertyEditorAlias);

    /// <summary>
    /// Parses index fields for a property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="culture">The culture to parse.</param>
    /// <param name="segment">The segment to parse.</param>
    /// <param name="published">Whether to parse the published or draft property value.</param>
    /// <param name="contentContext">The context in which the property exists.</param>
    /// <returns></returns>
    /// <remarks>
    /// <paramref name="contentContext"/> is included solely to contextualize the property value. Do not assume that the property is part of <paramref name="contentContext"/>.
    /// For example, properties in block editors will receive the "root content" on which the block editor itself resides as <paramref name="contentContext"/>.
    /// </remarks>
    IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext);
}
