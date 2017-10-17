using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a default implementation for <see cref="IPublishedContentTypeFactory"/>.
    /// </summary>
    internal class PublishedContentTypeFactory : IPublishedContentTypeFactory
    {
        private readonly IPublishedModelFactory _publishedModelFactory;
        private readonly PropertyValueConverterCollection _propertyValueConverters;
        private readonly IDataTypeConfigurationSource _dataTypeConfigurationSource;

        public PublishedContentTypeFactory(IPublishedModelFactory publishedModelFactory, PropertyValueConverterCollection propertyValueConverters, IDataTypeConfigurationSource dataTypeConfigurationSource)
        {
            _publishedModelFactory = publishedModelFactory;
            _propertyValueConverters = propertyValueConverters;
            _dataTypeConfigurationSource = dataTypeConfigurationSource;
        }

        /// <inheritdoc />
        public PublishedContentType CreateContentType(PublishedItemType itemType, IContentTypeComposition contentType)
        {
            return new PublishedContentType(itemType, contentType, this);
        }

        // for tests
        internal PublishedContentType CreateContentType(int id, string alias, IEnumerable<PublishedPropertyType> propertyTypes)
        {
            return new PublishedContentType(id, alias, propertyTypes, this);
        }

        // for tests
        internal PublishedContentType CreateContentType(int id, string alias, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes)
        {
            return new PublishedContentType(id, alias, compositionAliases, propertyTypes, this);
        }

        /// <inheritdoc />
        public PublishedPropertyType CreatePropertyType(PublishedContentType contentType, PropertyType propertyType)
        {
            return new PublishedPropertyType(contentType, propertyType, _publishedModelFactory, _propertyValueConverters, this);
        }

        /// <inheritdoc />
        public PublishedPropertyType CreatePropertyType(string propertyTypeAlias, int dataTypeId, string editorAlias, bool umbraco = false)
        {
            return new PublishedPropertyType(propertyTypeAlias, dataTypeId, editorAlias, umbraco, _publishedModelFactory, _propertyValueConverters, this);
        }

        /// <inheritdoc />
        public PublishedDataType CreateDataType(int id, string editorAlias)
        {
            return new PublishedDataType(id, editorAlias, _dataTypeConfigurationSource);
        }
    }
}
