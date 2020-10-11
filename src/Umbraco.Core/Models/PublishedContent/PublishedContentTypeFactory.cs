using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a default implementation for <see cref="IPublishedContentTypeFactory"/>.
    /// </summary>
    internal class PublishedContentTypeFactory : IPublishedContentTypeFactory
    {
        private readonly IPublishedModelFactory _publishedModelFactory;
        private readonly PropertyValueConverterCollection _propertyValueConverters;
        private readonly IDataTypeService _dataTypeService;
        private readonly object _publishedDataTypesLocker = new object();
        private Dictionary<int, PublishedDataType> _publishedDataTypes;

        public PublishedContentTypeFactory(IPublishedModelFactory publishedModelFactory, PropertyValueConverterCollection propertyValueConverters, IDataTypeService dataTypeService)
        {
            _publishedModelFactory = publishedModelFactory;
            _propertyValueConverters = propertyValueConverters;
            _dataTypeService = dataTypeService;
        }

        /// <inheritdoc />
        public IPublishedContentType CreateContentType(IContentTypeComposition contentType)
        {
            return new PublishedContentType(contentType, this);
        }

        /// <summary>
        /// This method is for tests and is not intended to be used directly from application code.
        /// </summary>
        /// <remarks>Values are assumed to be consisted and are not checked.</remarks>
        internal IPublishedContentType CreateContentType(Guid key, int id, string alias, Func<IPublishedContentType, IEnumerable<IPublishedPropertyType>> propertyTypes, ContentVariation variations = ContentVariation.Nothing, bool isElement = false)
        {
            return new PublishedContentType(key, id, alias, PublishedItemType.Content, Enumerable.Empty<string>(), propertyTypes, variations, isElement);
        }

        /// <summary>
        /// This method is for tests and is not intended to be used directly from application code.
        /// </summary>
        /// <remarks>Values are assumed to be consisted and are not checked.</remarks>
        internal IPublishedContentType CreateContentType(Guid key, int id, string alias, IEnumerable<string> compositionAliases, Func<IPublishedContentType, IEnumerable<IPublishedPropertyType>> propertyTypes, ContentVariation variations = ContentVariation.Nothing, bool isElement = false)
        {
            return new PublishedContentType(key, id, alias, PublishedItemType.Content, compositionAliases, propertyTypes, variations, isElement);
        }

        /// <inheritdoc />
        public IPublishedPropertyType CreatePropertyType(IPublishedContentType contentType, PropertyType propertyType)
        {
            return new PublishedPropertyType(contentType, propertyType, _propertyValueConverters, _publishedModelFactory, this);
        }

        /// <inheritdoc />
        public IPublishedPropertyType CreatePropertyType(IPublishedContentType contentType, string propertyTypeAlias, int dataTypeId, ContentVariation variations = ContentVariation.Nothing)
        {
            return new PublishedPropertyType(contentType, propertyTypeAlias, dataTypeId, true, variations, _propertyValueConverters, _publishedModelFactory, this);
        }

        /// <inheritdoc />
        public IPublishedPropertyType CreateCorePropertyType(IPublishedContentType contentType, string propertyTypeAlias, int dataTypeId, ContentVariation variations = ContentVariation.Nothing)
        {
            return new PublishedPropertyType(contentType, propertyTypeAlias, dataTypeId, false, variations, _propertyValueConverters, _publishedModelFactory, this);
        }

        /// <summary>
        /// This method is for tests and is not intended to be used directly from application code.
        /// </summary>
        /// <remarks>Values are assumed to be consisted and are not checked.</remarks>
        internal IPublishedPropertyType CreatePropertyType(string propertyTypeAlias, int dataTypeId, bool umbraco = false, ContentVariation variations = ContentVariation.Nothing)
        {
            return new PublishedPropertyType(propertyTypeAlias, dataTypeId, umbraco, variations, _propertyValueConverters, _publishedModelFactory, this);
        }

        /// <inheritdoc />
        public PublishedDataType GetDataType(int id)
        {
            Dictionary<int, PublishedDataType> publishedDataTypes;
            lock (_publishedDataTypesLocker)
            {
                if (_publishedDataTypes == null)
                {
                    var dataTypes = _dataTypeService.GetAll();
                    _publishedDataTypes = dataTypes.ToDictionary(x => x.Id, CreatePublishedDataType);
                }

                publishedDataTypes = _publishedDataTypes;
            }

            if (!publishedDataTypes.TryGetValue(id, out var dataType))
                throw new ArgumentException($"Could not find a datatype with identifier {id}.", nameof(id));

            return dataType;
        }

        /// <inheritdoc />
        public void NotifyDataTypeChanges(int[] ids)
        {
            lock (_publishedDataTypesLocker)
            {
                if (_publishedDataTypes == null)
                {
                    var dataTypes = _dataTypeService.GetAll();
                    _publishedDataTypes = dataTypes.ToDictionary(x => x.Id, CreatePublishedDataType);
                }
                else
                {
                    foreach (var id in ids)
                        _publishedDataTypes.Remove(id);

                    var dataTypes = _dataTypeService.GetAll(ids);
                    foreach (var dataType in dataTypes)
                        _publishedDataTypes[dataType.Id] = CreatePublishedDataType(dataType);
                }
            }
        }

        private PublishedDataType CreatePublishedDataType(IDataType dataType)
            => new PublishedDataType(dataType.Id, dataType.EditorAlias, dataType is DataType d ? d.GetLazyConfiguration() : new Lazy<object>(() => dataType.Configuration));
    }
}
