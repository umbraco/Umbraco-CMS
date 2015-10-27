using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Moq;
using NUnit.Framework;
using umbraco;
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
        private readonly Mock<IContentTypeService> _contentTypeService = new Mock<IContentTypeService>();
        private readonly Mock<IContentService> _contentService = new Mock<IContentService>();
        private readonly Mock<IDataTypeService> _dataTypeService = new Mock<IDataTypeService>();
        private Mock<PropertyEditorResolver> _propertyEditorResolver;
        private readonly Mock<IEntityService> _entityService = new Mock<IEntityService>();
        private readonly Mock<IFileService> _fileService = new Mock<IFileService>();

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
                    dataTypeService:_dataTypeService.Object,
                    entityService:_entityService.Object,
                    fileService: _fileService.Object),                    

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
                var entityMapper = new EntityModelMapper();
                entityMapper.ConfigureMappings(configuration, appContext);
            });
        }
        
        [Test]
        public void ContentTypeSave_To_IContentType()
        {
            //Arrange

            // setup the mocks to return the data we want to test against...
            
            _dataTypeService.Setup(x => x.GetDataTypeDefinitionById(It.IsAny<int>()))
                .Returns(Mock.Of<IDataTypeDefinition>(
                    definition => 
                        definition.Id == 555 
                        && definition.PropertyEditorAlias == "myPropertyType"
                        && definition.DatabaseType == DataTypeDatabaseType.Nvarchar));


            _fileService.Setup(x => x.GetTemplate(It.IsAny<string>()))
                .Returns((string alias) => Mock.Of<ITemplate>(
                    definition =>
                        definition.Id == alias.GetHashCode() && definition.Alias == alias));
                

            var display = CreateContentTypeSave();

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
            Assert.AreEqual(display.IsContainer, result.IsContainer);
            Assert.AreEqual(display.AllowAsRoot, result.AllowedAsRoot);
            Assert.AreEqual(display.CreateDate, result.CreateDate);
            Assert.AreEqual(display.UpdateDate, result.UpdateDate);
            
            //TODO: Now we need to assert all of the more complicated parts
            Assert.AreEqual(display.Groups.Count(), result.PropertyGroups.Count);
            for (var i = 0; i < display.Groups.Count(); i++)
            {
                Assert.AreEqual(display.Groups.ElementAt(i).Id, result.PropertyGroups.ElementAt(i).Id);
                Assert.AreEqual(display.Groups.ElementAt(i).Name, result.PropertyGroups.ElementAt(i).Name);
                var propTypes = display.Groups.ElementAt(i).Properties;
                Assert.AreEqual(propTypes.Count(), result.PropertyTypes.Count());
                for (var j = 0; j < propTypes.Count(); j++)
                {
                    Assert.AreEqual(propTypes.ElementAt(j).Id, result.PropertyTypes.ElementAt(j).Id);
                    Assert.AreEqual(propTypes.ElementAt(j).DataTypeId, result.PropertyTypes.ElementAt(j).DataTypeDefinitionId);
                }
            }

            var allowedTemplateAliases = display.AllowedTemplates
                .Concat(new[] {display.DefaultTemplate})
                .Distinct();

            Assert.AreEqual(allowedTemplateAliases.Count(), result.AllowedTemplates.Count());
            for (var i = 0; i < display.AllowedTemplates.Count(); i++)
            {
                Assert.AreEqual(display.AllowedTemplates.ElementAt(i), result.AllowedTemplates.ElementAt(i).Alias);
            }

            Assert.AreEqual(display.AllowedContentTypes.Count(), result.AllowedContentTypes.Count());
            for (var i = 0; i < display.AllowedContentTypes.Count(); i++)
            {
                Assert.AreEqual(display.AllowedContentTypes.ElementAt(i), result.AllowedContentTypes.ElementAt(i).Id.Value);
            }
        }

        [Test]
        public void ContentTypeSave_With_Composition_To_IContentType()
        {
            //Arrange

            // setup the mocks to return the data we want to test against...

            _dataTypeService.Setup(x => x.GetDataTypeDefinitionById(It.IsAny<int>()))
                .Returns(Mock.Of<IDataTypeDefinition>(
                    definition =>
                        definition.Id == 555
                        && definition.PropertyEditorAlias == "myPropertyType"
                        && definition.DatabaseType == DataTypeDatabaseType.Nvarchar));


            var display = CreateCompositionContentTypeSave();

            //Act

            var result = Mapper.Map<IContentType>(display);

            //Assert

            //TODO: Now we need to assert all of the more complicated parts
            Assert.AreEqual(display.Groups.Count(x => x.Inherited == false), result.PropertyGroups.Count);
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
            MockedContentTypes.EnsureAllIds(contentType, 8888);

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
            Assert.AreEqual(contentType.IsContainer, result.IsContainer);
            Assert.AreEqual(contentType.CreateDate, result.CreateDate);
            Assert.AreEqual(contentType.UpdateDate, result.UpdateDate);
            Assert.AreEqual(contentType.DefaultTemplate.Alias, result.DefaultTemplate.Alias);

            //TODO: Now we need to assert all of the more complicated parts

            Assert.AreEqual(contentType.PropertyGroups.Count(), result.Groups.Count());
            for (var i = 0; i < contentType.PropertyGroups.Count(); i++)
            {
                Assert.AreEqual(contentType.PropertyGroups.ElementAt(i).Id, result.Groups.ElementAt(i).Id);
                Assert.AreEqual(contentType.PropertyGroups.ElementAt(i).Name, result.Groups.ElementAt(i).Name);
                var propTypes = contentType.PropertyGroups.ElementAt(i).PropertyTypes;

                Assert.AreEqual(propTypes.Count(), result.Groups.ElementAt(i).Properties.Count());
                for (var j = 0; j < propTypes.Count(); j++)
                {
                    Assert.AreEqual(propTypes.ElementAt(j).Id, result.Groups.ElementAt(i).Properties.ElementAt(j).Id);
                    Assert.AreEqual(propTypes.ElementAt(j).DataTypeDefinitionId, result.Groups.ElementAt(i).Properties.ElementAt(j).DataTypeId);
                }
            }

            Assert.AreEqual(contentType.AllowedTemplates.Count(), result.AllowedTemplates.Count());
            for (var i = 0; i < contentType.AllowedTemplates.Count(); i++)
            {
                Assert.AreEqual(contentType.AllowedTemplates.ElementAt(i).Id, result.AllowedTemplates.ElementAt(i).Id);
            }

            Assert.AreEqual(contentType.AllowedContentTypes.Count(), result.AllowedContentTypes.Count());
            for (var i = 0; i < contentType.AllowedContentTypes.Count(); i++)
            {
                Assert.AreEqual(contentType.AllowedContentTypes.ElementAt(i).Id.Value, result.AllowedContentTypes.ElementAt(i));
            }

        }

        [Test]
        public void PropertyGroupBasic_To_PropertyGroup()
        {
            var basic = new PropertyGroupBasic<PropertyTypeBasic>()
            {
                Id = 222,
                Name = "Group 1",
                SortOrder = 1,
                Properties = new[]
                {
                    new PropertyTypeBasic()
                    {
                        Id = 33,
                        SortOrder = 1,
                        Alias = "prop1",
                        Description = "property 1",
                        DataTypeId = 99,
                        GroupId = 222,
                        Label = "Prop 1",
                        Validation = new PropertyTypeValidation()
                        {
                            Mandatory = true,
                            Pattern = null
                        }
                    },
                    new PropertyTypeBasic()
                    {
                        Id = 34,
                        SortOrder = 2,
                        Alias = "prop2",
                        Description = "property 2",
                        DataTypeId = 99,
                        GroupId = 222,
                        Label = "Prop 2",
                        Validation = new PropertyTypeValidation()
                        {
                            Mandatory = false,
                            Pattern = null
                        }
                    },
                }
            };

            var result = Mapper.Map<PropertyGroup>(basic);

            Assert.AreEqual(basic.Name, result.Name);
            Assert.AreEqual(basic.Id, result.Id);
            Assert.AreEqual(basic.SortOrder, result.SortOrder);
            Assert.AreEqual(basic.Properties.Count(), result.PropertyTypes.Count());
        }

        [Test]
        public void PropertyTypeBasic_To_PropertyType()
        {
            _dataTypeService.Setup(x => x.GetDataTypeDefinitionById(It.IsAny<int>()))
                .Returns(new DataTypeDefinition("test"));

            var basic = new PropertyTypeBasic()
            {
                Id = 33,
                SortOrder = 1,
                Alias = "prop1",
                Description = "property 1",
                DataTypeId = 99,
                GroupId = 222,
                Label = "Prop 1",
                Validation = new PropertyTypeValidation()
                {
                    Mandatory = true,
                    Pattern = "xyz"
                }
            };

            var result = Mapper.Map<PropertyType>(basic);

            Assert.AreEqual(basic.Id, result.Id);
            Assert.AreEqual(basic.SortOrder, result.SortOrder);
            Assert.AreEqual(basic.Alias, result.Alias);
            Assert.AreEqual(basic.Description, result.Description);
            Assert.AreEqual(basic.DataTypeId, result.DataTypeDefinitionId);           
            Assert.AreEqual(basic.Label, result.Name);
            Assert.AreEqual(basic.Validation.Mandatory, result.Mandatory);
            Assert.AreEqual(basic.Validation.Pattern, result.ValidationRegExp);
        }

        [Test]
        public void IContentTypeComposition_To_ContentTypeDisplay()
        {
            //Arrange

            // setup the mocks to return the data we want to test against...

            // for any call to GetPreValuesCollectionByDataTypeId just return an empty dictionary for now
            // TODO: but we'll need to change this to return some pre-values to test the mappings
            _dataTypeService.Setup(x => x.GetPreValuesCollectionByDataTypeId(It.IsAny<int>()))
                .Returns(new PreValueCollection(new Dictionary<string, PreValue>()));

            _entityService.Setup(x => x.GetObjectType(It.IsAny<int>()))
                .Returns(UmbracoObjectTypes.DocumentType);

            //return a textbox property editor for any requested editor by alias
            _propertyEditorResolver.Setup(resolver => resolver.GetByAlias(It.IsAny<string>()))
                .Returns(new TextboxPropertyEditor());
            //for testing, just return a list of whatever property editors we want
            _propertyEditorResolver.Setup(resolver => resolver.PropertyEditors)
                .Returns(new[] { new TextboxPropertyEditor() });

            var ctMain = MockedContentTypes.CreateSimpleContentType();
            //not assigned to tab
            ctMain.AddPropertyType(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext)
            {
                Alias = "umbracoUrlName", Name = "Slug", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            });
            MockedContentTypes.EnsureAllIds(ctMain, 8888);
            var ctChild1 = MockedContentTypes.CreateSimpleContentType("child1", "Child 1", ctMain, true);
            ctChild1.AddPropertyType(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext)
            {
                Alias = "someProperty",
                Name = "Some Property",
                Description = "",
                Mandatory = false,
                SortOrder = 1,
                DataTypeDefinitionId = -88
            }, "Another tab");
            MockedContentTypes.EnsureAllIds(ctChild1, 7777);
            var contentType = MockedContentTypes.CreateSimpleContentType("child2", "Child 2", ctChild1, true, "CustomGroup");
            //not assigned to tab
            contentType.AddPropertyType(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext)
            {
                Alias = "umbracoUrlAlias", Name = "AltUrl", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            });
            MockedContentTypes.EnsureAllIds(contentType, 6666);
            

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
            Assert.AreEqual(contentType.IsContainer, result.IsContainer);
            Assert.AreEqual(contentType.CreateDate, result.CreateDate);
            Assert.AreEqual(contentType.UpdateDate, result.UpdateDate);
            Assert.AreEqual(contentType.DefaultTemplate.Alias, result.DefaultTemplate.Alias);

            //TODO: Now we need to assert all of the more complicated parts

            Assert.AreEqual(contentType.CompositionPropertyGroups.Select(x => x.Name).Distinct().Count(), result.Groups.Count(x => x.Id != -666));
            Assert.AreEqual(1, result.Groups.Count(x => x.Id == -666));
            Assert.AreEqual(contentType.PropertyGroups.Count(), result.Groups.Count(x => x.Inherited == false && x.Id != -666));

            var allPropertiesMapped = result.Groups.SelectMany(x => x.Properties).ToArray();
            var allPropertyIdsMapped = allPropertiesMapped.Select(x => x.Id).ToArray();
            var allSourcePropertyIds = contentType.CompositionPropertyTypes.Select(x => x.Id).ToArray();

            Assert.AreEqual(contentType.PropertyTypes.Count(), allPropertiesMapped.Count(x => x.Inherited == false));
            Assert.AreEqual(allPropertyIdsMapped.Count(), allSourcePropertyIds.Count());
            Assert.IsTrue(allPropertyIdsMapped.ContainsAll(allSourcePropertyIds));

            Assert.AreEqual(2, result.Groups.Count(x => x.ParentTabContentTypes.Any()));
            Assert.IsTrue(result.Groups.SelectMany(x => x.ParentTabContentTypes).ContainsAll(new[] {ctMain.Id, ctChild1.Id}));

            Assert.AreEqual(contentType.AllowedTemplates.Count(), result.AllowedTemplates.Count());
            for (var i = 0; i < contentType.AllowedTemplates.Count(); i++)
            {
                Assert.AreEqual(contentType.AllowedTemplates.ElementAt(i).Id, result.AllowedTemplates.ElementAt(i).Id);
            }

            Assert.AreEqual(contentType.AllowedContentTypes.Count(), result.AllowedContentTypes.Count());
            for (var i = 0; i < contentType.AllowedContentTypes.Count(); i++)
            {
                Assert.AreEqual(contentType.AllowedContentTypes.ElementAt(i).Id.Value, result.AllowedContentTypes.ElementAt(i));
            }

        }


        private ContentTypeSave CreateContentTypeSave()
        {            
            return new ContentTypeSave
            {
                Alias = "test",     
                AllowAsRoot = true,
                AllowedTemplates = new []
                {
                    "template1",
                    "template2"
                },
                AllowedContentTypes = new [] {666, 667},
                DefaultTemplate = "test",
                Description = "hello world",
                Icon = "tree-icon",
                Id = 1234,
                Key = new Guid("8A60656B-3866-46AB-824A-48AE85083070"),
                Name = "My content type",
                Path = "-1,1234",
                ParentId = -1,
                Thumbnail = "tree-thumb",
                IsContainer = true,
                Groups = new []
                {
                    new PropertyGroupBasic<PropertyTypeBasic>()
                    {
                        Id = 987,
                        Name = "Tab 1",
                        SortOrder = 0,
                        Inherited = false,                        
                        Properties = new []
                        {
                            new PropertyTypeBasic
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
                                SortOrder = 0,
                                DataTypeId = 555
                            }
                        }
                    }
                }
            };
        }

        private ContentTypeSave CreateCompositionContentTypeSave()
        {
            return new ContentTypeSave
            {
                Alias = "test",
                AllowAsRoot = true,
                AllowedTemplates = new[]
                {
                    "template1",
                    "template2"
                },
                AllowedContentTypes = new[] { 666, 667 },
                DefaultTemplate = "test",
                Description = "hello world",
                Icon = "tree-icon",
                Id = 1234,
                Key = new Guid("8A60656B-3866-46AB-824A-48AE85083070"),
                Name = "My content type",
                Path = "-1,1234",
                ParentId = -1,
                Thumbnail = "tree-thumb",
                IsContainer = true,
                Groups = new[]
                {
                    new PropertyGroupBasic<PropertyTypeBasic>()
                    {
                        Id = 987,
                        Name = "Tab 1",
                        SortOrder = 0,
                        Inherited = false,                        
                        Properties = new[]
                        {
                            new PropertyTypeBasic
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
                                SortOrder = 0,
                                DataTypeId = 555
                            }
                        }
                    },
                    new PropertyGroupBasic<PropertyTypeBasic>()
                    {
                        Id = 894,
                        Name = "Tab 2",
                        SortOrder = 0,
                        Inherited = true,                        
                        Properties = new[]
                        {
                            new PropertyTypeBasic
                            {                                
                                Alias = "parentProperty",
                                Description = "this is a property from the parent",
                                Inherited = true,
                                Label = "Parent property",
                                Validation = new PropertyTypeValidation
                                {
                                    Mandatory = false,
                                    Pattern = ""
                                },                                
                                SortOrder = 0,
                                DataTypeId = 555
                            }
                        }
                        
                    }
                }

            };
        }
    }
}
