using System.Collections.Generic;
using System.Linq;
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
        public PublishedContentType CreateContentType(IContentTypeComposition contentType)
        {
            return new PublishedContentType(contentType, this);
        }

        // for tests - fixme what's the point of the factory here?
        internal PublishedContentType CreateContentType(int id, string alias, IEnumerable<PublishedPropertyType> propertyTypes, ContentVariation variations = ContentVariation.InvariantNeutral)
        {
            return new PublishedContentType(id, alias, PublishedItemType.Content, Enumerable.Empty<string>(), propertyTypes, variations);
        }

        // for tests - fixme what's the point of the factory here?
        internal PublishedContentType CreateContentType(int id, string alias, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes, ContentVariation variations = ContentVariation.InvariantNeutral)
        {
            return new PublishedContentType(id, alias, PublishedItemType.Content, compositionAliases, propertyTypes, variations);
        }

        /// <inheritdoc />
        public PublishedPropertyType CreatePropertyType(PublishedContentType contentType, PropertyType propertyType)
        {
            return new PublishedPropertyType(contentType, propertyType, _propertyValueConverters, _publishedModelFactory, this);
        }

        /// <inheritdoc />
        public PublishedPropertyType CreatePropertyType(PublishedContentType contentType, string propertyTypeAlias, int dataTypeId, string propertyEditorAlias, ContentVariation variations = ContentVariation.InvariantNeutral)
        {
            return new PublishedPropertyType(contentType, propertyTypeAlias, dataTypeId, propertyEditorAlias, true, variations, _propertyValueConverters, _publishedModelFactory, this);
        }

        // for tests
        internal PublishedPropertyType CreatePropertyType(string propertyTypeAlias, int dataTypeId, string propertyEditorAlias, bool umbraco = false, ContentVariation variations = ContentVariation.InvariantNeutral)
        {
            return new PublishedPropertyType(propertyTypeAlias, dataTypeId, propertyEditorAlias, umbraco, variations, _propertyValueConverters, _publishedModelFactory, this);
        }

        /// <inheritdoc />
        public PublishedDataType CreateDataType(int id, string editorAlias)
        {
            return new PublishedDataType(id, editorAlias, _dataTypeConfigurationSource);
        }
    }
}
