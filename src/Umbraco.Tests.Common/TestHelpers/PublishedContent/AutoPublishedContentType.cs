// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Common.TestHelpers.PublishedContent
{

    public class AutoPublishedContentType : PublishedContentType
    {
        private static readonly IPublishedPropertyType Default;

        static AutoPublishedContentType()
        {
            var configurationEditorJsonSerializer = new ConfigurationEditorJsonSerializer();
            var jsonSerializer = new JsonNetSerializer();
            var dataTypeServiceMock = new Mock<IDataTypeService>();

            var dataType = new DataType(
                new VoidEditor(
                    Mock.Of<IDataValueEditorFactory>()),
                configurationEditorJsonSerializer)
            {
                Id = 666
            };
            dataTypeServiceMock.Setup(x => x.GetAll()).Returns(dataType.Yield);

            var factory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), new PropertyValueConverterCollection(() => Enumerable.Empty<IPropertyValueConverter>()), dataTypeServiceMock.Object);
            Default = factory.CreatePropertyType("*", 666);
        }

        public AutoPublishedContentType(Guid key, int id, string alias, IEnumerable<PublishedPropertyType> propertyTypes)
            : base(key, id, alias, PublishedItemType.Content, Enumerable.Empty<string>(), propertyTypes, ContentVariation.Nothing)
        {
        }

        public AutoPublishedContentType(Guid key, int id, string alias, Func<IPublishedContentType, IEnumerable<IPublishedPropertyType>> propertyTypes)
            : base(key, id, alias, PublishedItemType.Content, Enumerable.Empty<string>(), propertyTypes, ContentVariation.Nothing)
        {
        }

        public AutoPublishedContentType(Guid key, int id, string alias, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes)
            : base(key, id, alias, PublishedItemType.Content, compositionAliases, propertyTypes, ContentVariation.Nothing)
        {
        }

        public AutoPublishedContentType(Guid key, int id, string alias, IEnumerable<string> compositionAliases, Func<IPublishedContentType, IEnumerable<IPublishedPropertyType>> propertyTypes)
            : base(key, id, alias, PublishedItemType.Content, compositionAliases, propertyTypes, ContentVariation.Nothing)
        {
        }

        public override IPublishedPropertyType GetPropertyType(string alias)
        {
            IPublishedPropertyType propertyType = base.GetPropertyType(alias);
            return propertyType ?? Default;
        }
    }
}
