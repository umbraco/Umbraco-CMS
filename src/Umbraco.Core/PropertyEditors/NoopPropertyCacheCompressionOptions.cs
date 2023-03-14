using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Default implementation for <see cref="IPropertyCacheCompressionOptions" /> which does not compress any property
///     data
/// </summary>
public sealed class NoopPropertyCacheCompressionOptions : IPropertyCacheCompressionOptions
{
    public bool IsCompressed(IReadOnlyContentBase content, IPropertyType propertyType, IDataEditor dataEditor, bool published) => false;
}
