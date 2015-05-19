using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Moq;
using NUnit.Framework;
using umbraco.cms.presentation;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.Models.Mapping
{
    [TestFixture]
    public class ContentTypeModelMappingTests : BaseUmbracoConfigurationTest
    {
        //Mocks of services that can be setup on a test by test basis to return whatever we want
        private Mock<IContentTypeService> _contentTypeService = new Mock<IContentTypeService>();
        private Mock<IContentService> _contentService = new Mock<IContentService>();
        private Mock<IDataTypeService> _dataTypeService = new Mock<IDataTypeService>();
        private Mock<PropertyEditorResolver> _propertyEditorResolver;

        [SetUp]
        public void Setup()
        {
            var nullCacheHelper = CacheHelper.CreateDisabledCacheHelper();

            //Create an app context using mocks
            var appContext = new ApplicationContext(
                new DatabaseContext(Mock.Of<IDatabaseFactory>(), Mock.Of<ILogger>(), Mock.Of<ISqlSyntaxProvider>(), "test"),
                
                //Create service context using mocks
                new ServiceContext(

                    contentService: _contentService.Object,
                    contentTypeService:_contentTypeService.Object,
                    dataTypeService:_dataTypeService.Object),                    

                nullCacheHelper,
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));
            
            //create a fake property editor resolver to return fake property editors
            Func<IEnumerable<Type>> typeListProducerList = Enumerable.Empty<Type>;
            _propertyEditorResolver = new Mock<PropertyEditorResolver>(
                //ctor args
                Mock.Of<IServiceProvider>(), Mock.Of<ILogger>(), typeListProducerList, nullCacheHelper.RuntimeCache);
            
            Mapper.Initialize(configuration =>
            {
                //initialize our content type mapper
                var mapper = new ContentTypeModelMapper(new Lazy<PropertyEditorResolver>(() => _propertyEditorResolver.Object));
                mapper.ConfigureMappings(configuration, appContext);                
            });
        }

        [Test]
        public void ContentTypeDisplay_To_IContentType()
        {
            //Arrange

            // setup the mocks to return the data we want to test against...
            
            _dataTypeService.Setup(x => x.GetDataTypeDefinitionById(It.IsAny<int>()))
                .Returns(Mock.Of<IDataTypeDefinition>(
                    definition => 
                        definition.Id == 555 
                        && definition.PropertyEditorAlias == "myPropertyType"
                        && definition.DatabaseType == DataTypeDatabaseType.Nvarchar));

            var display = CreateSimpleContentTypeDisplay();

            //Act

            var result = Mapper.Map<IContentType>(display);

            //Assert

            Assert.AreEqual(display.Alias, result.Alias);
            Assert.AreEqual(display.Description, result.Description);
            Assert.AreEqual(display.Icon, result.Icon);
            Assert.AreEqual(display.Id, result.Id);
            Assert.AreEqual(display.Name, result.Name);
            Assert.AreEqual(display.ParentId, result.ParentId);
            Assert.AreEqual(display.Path, result.Path);
            Assert.AreEqual(display.Thumbnail, result.Thumbnail);
            Assert.AreEqual(display.AllowedAsRoot, result.AllowedAsRoot);
            Assert.AreEqual(display.EnableListView, result.IsContainer);
            
            //TODO: Now we need to assert all of the more complicated parts
            Assert.AreEqual(1, result.PropertyGroups.Count);
            Assert.AreEqual(1, result.PropertyGroups[0].PropertyTypes.Count);
        }

        [Test]
        public void IContentType_To_ContentTypeDisplay()
        {
            //Arrange

            // setup the mocks to return the data we want to test against...

            // for any call to GetPreValuesCollectionByDataTypeId just return an empty dictionary for now
            // TODO: but we'll need to change this to return some pre-values to test the mappings
            _dataTypeService.Setup(x => x.GetPreValuesCollectionByDataTypeId(It.IsAny<int>()))
                .Returns(new PreValueCollection(new Dictionary<string, PreValue>()));

            //return a textbox property editor for any requested editor by alias
            _propertyEditorResolver.Setup(resolver => resolver.GetByAlias(It.IsAny<string>()))
                .Returns(new TextboxPropertyEditor());
            //for testing, just return a list of whatever property editors we want
            _propertyEditorResolver.Setup(resolver => resolver.PropertyEditors)
                .Returns(new[] { new TextboxPropertyEditor() });
            
            var contentType = MockedContentTypes.CreateTextpageContentType();

            //Act

            var result = Mapper.Map<ContentTypeDisplay>(contentType);

            //Assert

            Assert.AreEqual(contentType.Alias, result.Alias);
            Assert.AreEqual(contentType.Description, result.Description);
            Assert.AreEqual(contentType.Icon, result.Icon);
            Assert.AreEqual(contentType.Id, result.Id);
            Assert.AreEqual(contentType.Name, result.Name);
            Assert.AreEqual(contentType.ParentId, result.ParentId);
            Assert.AreEqual(contentType.Path, result.Path);
            Assert.AreEqual(contentType.Thumbnail, result.Thumbnail);
            Assert.AreEqual(contentType.AllowedAsRoot, result.AllowedAsRoot);
            Assert.AreEqual(contentType.IsContainer, result.EnableListView);

            Assert.AreEqual(contentType.DefaultTemplate.Alias, result.DefaultTemplate.Alias);

            //TODO: Now we need to assert all of the more complicated parts
            Assert.AreEqual(2, result.Groups.Count());
            Assert.AreEqual(2, result.Groups.ElementAt(0).Properties.Count());
            Assert.AreEqual(2, result.Groups.ElementAt(1).Properties.Count());
        }

        private ContentTypeDisplay CreateSimpleContentTypeDisplay()
        {            
            return new ContentTypeDisplay
            {
                Alias = "test",                
                AllowedParentNodeTypes = new List<EntityBasic>(),
                AllowedTemplates = new List<EntityBasic>(),
                AvailableContentTypes = new List<EntityBasic>(),
                AvailableTemplates = new List<EntityBasic>(),
                DefaultTemplate = new EntityBasic(){ Alias = "test" },
                Description = "hello world",
                Icon = "tree-icon",
                Id = 1234,
                Key = new Guid("8A60656B-3866-46AB-824A-48AE85083070"),
                Name = "My content type",
                Path = "-1,1234",
                ParentId = -1,
                Thumbnail = "tree-thumb",
                EnableListView = true,
                Groups = new List<PropertyTypeGroupDisplay>()
                {
                    new PropertyTypeGroupDisplay
                    {
                        Id = 987,
                        Name = "Tab 1",
                        ParentGroupId = -1,
                        SortOrder = 0,
                        Inherited = false,
                        Properties = new List<PropertyTypeDisplay>
                        {
                            new PropertyTypeDisplay
                            {                                
                                Alias = "property1",
                                Description = "this is property 1",
                                Inherited = false,
                                Label = "Property 1",
                                Validation = new PropertyTypeValidation
                                {
                                    Mandatory = false,
                                    Pattern = ""
                                },
                                Editor = "myPropertyType",
                                Value = "value 1",
                                //View = ??? - isn't this the same as editor?
                                Config = new Dictionary<string, object>
                                {
                                    {"item1", "value1"},
                                    {"item2", "value2"}
                                },
                                SortOrder = 0,
                                DataTypeId = 555,
                                View = "blah"
                            }
                        }
                    }
                }

            };
        }
    }
}
