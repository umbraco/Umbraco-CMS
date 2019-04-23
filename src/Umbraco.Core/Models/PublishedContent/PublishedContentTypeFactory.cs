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
    public class PublishedContentTypeFactory : IPublishedContentTypeFactory
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
        public PublishedContentType CreateContentType(IContentTypeComposition contentType)
        {
            return new PublishedContentType(contentType, this);
        }

        // for tests
        /// <summary>
        /// Only for unit tests
        /// REVIEW: Make protected?
        /// </summary>
        /// <param name="id"></param>
        /// <param name="alias"></param>
        /// <param name="propertyTypes"></param>
        /// <param name="variations"></param>
        /// <param name="isElement"></param>
        /// <returns></returns>
        public PublishedContentType CreateContentType(int id, string alias, IEnumerable<PublishedPropertyType> propertyTypes, ContentVariation variations = ContentVariation.Nothing, bool isElement = false)
        {
            return new PublishedContentType(id, alias, PublishedItemType.Content, Enumerable.Empty<string>(), propertyTypes, variations, isElement);
        }

        // for tests
        /// <summary>
        /// Only for unit tests
        /// REVIEW: Make protected?
        /// </summary>
        /// <param name="id"></param>
        /// <param name="alias"></param>
        /// <param name="propertyTypes"></param>
        /// <param name="variations"></param>
        /// <param name="isElement"></param>
        /// <returns></returns>
        public PublishedContentType CreateContentType(int id, string alias, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes, ContentVariation variations = ContentVariation.Nothing, bool isElement = false)
        {
            return new PublishedContentType(id, alias, PublishedItemType.Content, compositionAliases, propertyTypes, variations, isElement);
        }

        /// <inheritdoc />
        public PublishedPropertyType CreatePropertyType(PublishedContentType contentType, PropertyType propertyType)
        {
            return new PublishedPropertyType(contentType, propertyType, _propertyValueConverters, _publishedModelFactory, this);
        }

        /// <inheritdoc />
        public PublishedPropertyType CreatePropertyType(PublishedContentType contentType, string propertyTypeAlias, int dataTypeId, ContentVariation variations = ContentVariation.Nothing)
        {
            return new PublishedPropertyType(contentType, propertyTypeAlias, dataTypeId, true, variations, _propertyValueConverters, _publishedModelFactory, this);
        }

        /// <summary>
        /// For unit testing only
        /// REVIEW: Make protected? - AKA Should this possibly be made public in "adapter class" or from class in test lib? (or should tests just pass the content type?)
        /// TODO: Property type alias
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="dataTypeId"></param>
        /// <param name="umbraco"></param>
        /// <param name="variations"></param>
        /// <returns></returns>
        public PublishedPropertyType CreatePropertyType(string propertyTypeAlias, int dataTypeId, bool umbraco = false, ContentVariation variations = ContentVariation.Nothing)
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
