using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes multi-node tree picker property values as the picked documents' keys (Keywords). Picker configurations
/// targeting non-document object types (e.g. media) are not indexed.
/// </summary>
internal sealed class MultiNodeTreePickerPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IDataTypeConfigurationCache _dataTypeConfigurationCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiNodeTreePickerPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="dataTypeConfigurationCache">The cache used to resolve the picker's configured tree source object type.</param>
    public MultiNodeTreePickerPropertyValueHandler(IDataTypeConfigurationCache dataTypeConfigurationCache)
        => _dataTypeConfigurationCache = dataTypeConfigurationCache;

    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.MultiNodeTreePicker;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        MultiNodePickerConfiguration? configuration = _dataTypeConfigurationCache.GetConfigurationAs<MultiNodePickerConfiguration>(property.PropertyType.DataTypeKey);

        // NOTES:
        // - the default configuration for MNTP has ObjectType null, which is inferred as a document picker
        // - the DocumentObjectType is an internal constant in Umbraco 16 - value is "content"
        if (configuration?.TreeSource?.ObjectType is not (null or "content"))
        {
            return [];
        }

        var value = property.GetValue(culture, segment, published) as string;
        if (value.IsNullOrWhiteSpace())
        {
            return [];
        }

        var keysAsKeywords = value
            .Split(Umbraco.Cms.Core.Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(v => UdiParser.TryParse(v, out Udi? udi)
                         && udi is GuidUdi { EntityType: Umbraco.Cms.Core.Constants.UdiEntityType.Document } guidUdi
                ? guidUdi.Guid.AsKeyword()
                : null)
            .WhereNotNull()
            .ToArray();

        return keysAsKeywords.Length > 0
            ? [new IndexField(property.Alias, new IndexValue { Keywords = keysAsKeywords }, culture, segment)]
            : [];
    }
}
