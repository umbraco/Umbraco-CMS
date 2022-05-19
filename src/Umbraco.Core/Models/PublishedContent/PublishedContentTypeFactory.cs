using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Provides a default implementation for <see cref="IPublishedContentTypeFactory" />.
/// </summary>
public class PublishedContentTypeFactory : IPublishedContentTypeFactory
{
    private readonly IDataTypeService _dataTypeService;
    private readonly PropertyValueConverterCollection _propertyValueConverters;
    private readonly object _publishedDataTypesLocker = new();
    private readonly IPublishedModelFactory _publishedModelFactory;
    private Dictionary<int, PublishedDataType>? _publishedDataTypes;

    public PublishedContentTypeFactory(
        IPublishedModelFactory publishedModelFactory,
        PropertyValueConverterCollection propertyValueConverters,
        IDataTypeService dataTypeService)
    {
        _publishedModelFactory = publishedModelFactory;
        _propertyValueConverters = propertyValueConverters;
        _dataTypeService = dataTypeService;
    }

    /// <inheritdoc />
    public IPublishedContentType CreateContentType(IContentTypeComposition contentType) =>
        new PublishedContentType(contentType, this);

    /// <inheritdoc />
    public IPublishedPropertyType CreatePropertyType(IPublishedContentType contentType, IPropertyType propertyType) =>
        new PublishedPropertyType(contentType, propertyType, _propertyValueConverters, _publishedModelFactory, this);

    /// <inheritdoc />
    public IPublishedPropertyType CreatePropertyType(
        IPublishedContentType contentType,
        string propertyTypeAlias,
        int dataTypeId,
        ContentVariation variations = ContentVariation.Nothing) =>
        new PublishedPropertyType(
        contentType, propertyTypeAlias, dataTypeId, true, variations, _propertyValueConverters, _publishedModelFactory, this);

    /// <inheritdoc />
    public IPublishedPropertyType CreateCorePropertyType(
        IPublishedContentType contentType,
        string propertyTypeAlias,
        int dataTypeId,
        ContentVariation variations = ContentVariation.Nothing) =>
        new PublishedPropertyType(contentType, propertyTypeAlias, dataTypeId, false, variations, _propertyValueConverters, _publishedModelFactory, this);

    /// <inheritdoc />
    public PublishedDataType GetDataType(int id)
    {
        Dictionary<int, PublishedDataType>? publishedDataTypes;
        lock (_publishedDataTypesLocker)
        {
            if (_publishedDataTypes == null)
            {
                IEnumerable<IDataType> dataTypes = _dataTypeService.GetAll();
                _publishedDataTypes = dataTypes.ToDictionary(x => x.Id, CreatePublishedDataType);
            }

            publishedDataTypes = _publishedDataTypes;
        }

        if (publishedDataTypes is null || !publishedDataTypes.TryGetValue(id, out PublishedDataType? dataType))
        {
            throw new ArgumentException($"Could not find a datatype with identifier {id}.", nameof(id));
        }

        return dataType;
    }

    /// <inheritdoc />
    public void NotifyDataTypeChanges(int[] ids)
    {
        lock (_publishedDataTypesLocker)
        {
            if (_publishedDataTypes == null)
            {
                IEnumerable<IDataType> dataTypes = _dataTypeService.GetAll();
                _publishedDataTypes = dataTypes.ToDictionary(x => x.Id, CreatePublishedDataType);
            }
            else
            {
                foreach (var id in ids)
                {
                    _publishedDataTypes.Remove(id);
                }

                IEnumerable<IDataType> dataTypes = _dataTypeService.GetAll(ids);
                foreach (IDataType dataType in dataTypes)
                {
                    _publishedDataTypes[dataType.Id] = CreatePublishedDataType(dataType);
                }
            }
        }
    }

    /// <summary>
    ///     This method is for tests and is not intended to be used directly from application code.
    /// </summary>
    /// <remarks>Values are assumed to be consisted and are not checked.</remarks>
    internal IPublishedContentType CreateContentType(
        Guid key,
        int id,
        string alias,
        Func<IPublishedContentType, IEnumerable<IPublishedPropertyType>> propertyTypes,
        ContentVariation variations = ContentVariation.Nothing,
        bool isElement = false) =>
        new PublishedContentType(key, id, alias, PublishedItemType.Content, Enumerable.Empty<string>(), propertyTypes, variations, isElement);

    /// <summary>
    ///     This method is for tests and is not intended to be used directly from application code.
    /// </summary>
    /// <remarks>Values are assumed to be consisted and are not checked.</remarks>
    internal IPublishedContentType CreateContentType(
        Guid key,
        int id,
        string alias,
        IEnumerable<string> compositionAliases,
        Func<IPublishedContentType, IEnumerable<IPublishedPropertyType>> propertyTypes,
        ContentVariation variations = ContentVariation.Nothing,
        bool isElement = false) =>
        new PublishedContentType(key, id, alias, PublishedItemType.Content, compositionAliases, propertyTypes, variations, isElement);

    /// <summary>
    ///     This method is for tests and is not intended to be used directly from application code.
    /// </summary>
    /// <remarks>Values are assumed to be consisted and are not checked.</remarks>
    internal IPublishedPropertyType CreatePropertyType(
        string propertyTypeAlias,
        int dataTypeId,
        bool umbraco = false,
        ContentVariation variations = ContentVariation.Nothing) =>
        new PublishedPropertyType(propertyTypeAlias, dataTypeId, umbraco, variations, _propertyValueConverters, _publishedModelFactory, this);

    private PublishedDataType CreatePublishedDataType(IDataType dataType)
        => new(dataType.Id, dataType.EditorAlias, dataType is DataType d ? d.GetLazyConfiguration() : new Lazy<object?>(() => dataType.Configuration));
}
