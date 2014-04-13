using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Serialization;
using Umbraco.Tests.CodeFirst.Definitions;
using Umbraco.Tests.CodeFirst.TestModels;
using Umbraco.Tests.CodeFirst.TestModels.Composition;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.CodeFirst
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class CodeFirstTests : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            
            //LegacyUmbracoSettings.SettingsFilePath = IOHelper.MapPath(SystemDirectories.Config + Path.DirectorySeparatorChar, false);
           
            base.Initialize();

            var jsonNetSerializer = new JsonNetSerializer();
            SerializationService = new SerializationService(jsonNetSerializer);
        }

        [Ignore("With the changes to the data type definition GUID -> Alias this no longer passes, will need Morten's help with this one")]
        [Test]
        public void Can_Create_Model_With_NonExisting_DataTypeDefinition()
        {
            ContentTypeDefinitionFactory.ClearContentTypeCache();

            var modelType = typeof(ModelWithNewDataType);
            var contentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(modelType);

            var mappedContentTypes = ContentTypeDefinitionFactory.RetrieveMappedContentTypes();
            ServiceContext.ContentTypeService.Save(mappedContentTypes);

            var model = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed + 1);
            Assert.That(model, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_ContentType_From_Decorated_Home_Model()
        {
            var modelType = typeof(Home);
            var contentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(modelType);

            Assert.That(contentType, Is.Not.Null);
            Assert.That(contentType.Value.PropertyGroups, Is.Not.Null);
            Assert.That(contentType.Value.PropertyTypes.Any(), Is.True);
            Assert.That(contentType.Value.PropertyTypes.Count(), Is.EqualTo(2));

            var result = SerializationService.ToStream(contentType.Value);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }

        [Test]
        public void Can_Resolve_ContentType_From_Decorated_ContentPage_Model()
        {
            var modelType = typeof(ContentPage);
            var contentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(modelType);

            Assert.That(contentType, Is.Not.Null);
            Assert.That(contentType.Value.PropertyGroups, Is.Not.Null);
            Assert.That(contentType.Value.PropertyGroups.Any(), Is.True);
            Assert.That(contentType.Value.PropertyGroups.Count(), Is.EqualTo(1));
            Assert.That(contentType.Value.PropertyTypes.Any(), Is.True);
            Assert.That(contentType.Value.PropertyTypes.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Can_Resolve_ContentType_From_PlainPocoType_Model()
        {
            var modelType = typeof(PlainPocoType);
            var contentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(modelType);

            Assert.That(contentType, Is.Not.Null);
            Assert.That(contentType.Value.PropertyGroups, Is.Not.Null);
            Assert.That(contentType.Value.PropertyTypes.Any(), Is.True);
            Assert.That(contentType.Value.PropertyGroups.Count(), Is.EqualTo(1));
            Assert.That(contentType.Value.PropertyTypes.Count(), Is.EqualTo(5));

            var result = SerializationService.ToStream(contentType.Value);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }

        [Test]
        public void Can_Retrieve_ContentTypes_After_Resolving()
        {
            ContentTypeDefinitionFactory.ClearContentTypeCache();

            var modelType = typeof(Home);
            var contentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(modelType);
            var mappedContentTypes = ContentTypeDefinitionFactory.RetrieveMappedContentTypes();

            Assert.That(mappedContentTypes, Is.Not.Null);
            Assert.That(mappedContentTypes.Any(), Is.True);
            Assert.That(mappedContentTypes.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Can_Resolve_Existing_ContentType_With_Decorated_Model()
        {
            var textPage = MockedContentTypes.CreateTextpageContentType();
            ServiceContext.ContentTypeService.Save(textPage);

            var modelType = typeof(TextPage);
            var contentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(modelType);

            Assert.That(contentType.Value.Id, Is.EqualTo(textPage.Id));

            ServiceContext.ContentTypeService.Save(contentType.Value);
        }

        [Test]
        public void Can_Save_Models_To_Database()
        {
            ContentTypeDefinitionFactory.ClearContentTypeCache();

            var homeModel = typeof(Home);
            var textPageModel = typeof(TextPage);
            var homeContentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(homeModel);
            var textPageContentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(textPageModel);

            var mappedContentTypes = ContentTypeDefinitionFactory.RetrieveMappedContentTypes().ToList();
            ServiceContext.ContentTypeService.Save(mappedContentTypes);
        }

        [Test]
        public void Can_Resolve_Parent_Child_ContentTypes_And_Save_To_Database()
        {
            ContentTypeDefinitionFactory.ClearContentTypeCache();

            var simplemodel = typeof(SimpleContentPage);
            var model = typeof(AdvancedContentPage);
            var sContentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(simplemodel);
            var aContentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(model);

            var mappedContentTypes = ContentTypeDefinitionFactory.RetrieveMappedContentTypes().ToList();
            ServiceContext.ContentTypeService.Save(mappedContentTypes);

            var type1 = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed);
            var type2 = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed + 1);

            Assert.That(type1, Is.Not.Null);
            Assert.That(type2, Is.Not.Null);
        }

        [Test]
        public void Can_Resolve_And_Save_Decorated_Model_To_Database()
        {
            ContentTypeDefinitionFactory.ClearContentTypeCache();

            var model = typeof(DecoratedModelPage);
            var modelContentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(model);

            var mappedContentTypes = ContentTypeDefinitionFactory.RetrieveMappedContentTypes().ToList();
            ServiceContext.ContentTypeService.Save(mappedContentTypes);

            var type1 = ServiceContext.ContentTypeService.GetContentType(NodeDto.NodeIdSeed + 2);

            Assert.That(type1, Is.Not.Null);
            Assert.That(type1.PropertyGroups.Count(), Is.EqualTo(2));
            Assert.That(type1.PropertyTypes.Count(), Is.EqualTo(4));

        }

        [Test]
        public void Can_Resolve_ContentType_Composition_And_Save_To_Database()
        {
            ContentTypeDefinitionFactory.ClearContentTypeCache();

            var metaSeoModel = typeof(MetaSeo);
            var seoContentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(metaSeoModel);
            var metaModel = typeof(Meta);
            var metaContentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(metaModel);
            var baseModel = typeof(Base);
            var baseContentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(baseModel);
            var newsModel = typeof(News);
            var newsContentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(newsModel);

            var mappedContentTypes = ContentTypeDefinitionFactory.RetrieveMappedContentTypes().ToList();
            ServiceContext.ContentTypeService.Save(mappedContentTypes);

            Assert.That(mappedContentTypes.Count(), Is.EqualTo(4));
        }

        [Ignore("This now fails due to the new constraints on the db tables: A duplicate value cannot be inserted into a unique index. [ Table name = cmsPropertyTypeGroup,Constraint name = PK_cmsPropertyTypeGroup ]")]
        [Test]
        public void Can_Resolve_Full_List_Of_Models_Implementing_ContentTypeBase()
        {
            ContentTypeDefinitionFactory.ClearContentTypeCache();

            var foundTypes = PluginManager.Current.ResolveContentTypeBaseTypes();
            var contentTypeList = foundTypes.Select(ContentTypeDefinitionFactory.GetContentTypeDefinition).ToList();

            var mappedContentTypes = ContentTypeDefinitionFactory.RetrieveMappedContentTypes();

            Assert.That(contentTypeList.Count(), Is.EqualTo(mappedContentTypes.Count()));

            ServiceContext.ContentTypeService.Save(mappedContentTypes);//Save to db
        }

        private SerializationService SerializationService { get; set; }

        [TearDown]
        public override void TearDown()
        {
			base.TearDown();
            
            SerializationService = null;
        }
    }
}