using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Serialization;
using Umbraco.Tests.CodeFirst.Attributes;
using Umbraco.Tests.CodeFirst.Definitions;
using Umbraco.Tests.CodeFirst.TestModels;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using umbraco.editorControls.tinyMCE3;
using umbraco.interfaces;

namespace Umbraco.Tests.CodeFirst
{
    [TestFixture]
    public class CodeFirstTests : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            UmbracoSettings.SettingsFilePath = IOHelper.MapPath(SystemDirectories.Config + Path.DirectorySeparatorChar, false);

            //this ensures its reset
            PluginManager.Current = new PluginManager();

            //for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
            PluginManager.Current.AssembliesToScan = new[]
				{
                    typeof(IDataType).Assembly,
                    typeof(tinyMCE3dataType).Assembly
				};

            DataTypesResolver.Current = new DataTypesResolver(
                PluginManager.Current.ResolveDataTypes());

            base.Initialize();

            var serviceStackSerializer = new ServiceStackJsonSerializer();
            SerializationService = new SerializationService(serviceStackSerializer);
        }

        [Test]
        public void Can_Create_Model_With_NonExisting_DataTypeDefinition()
        {
            ContentTypeDefinitionFactory.ClearContentTypeCache();

            var modelType = typeof(ModelWithNewDataType);
            var contentType = ContentTypeDefinitionFactory.GetContentTypeDefinition(modelType);

            var mappedContentTypes = ContentTypeDefinitionFactory.RetrieveMappedContentTypes();
            ServiceContext.ContentTypeService.Save(mappedContentTypes);

            var model = ServiceContext.ContentTypeService.GetContentType(1046);
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
            var xml = result.ResultStream.ToJsonString();
            Console.WriteLine(xml);
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
            var xml = result.ResultStream.ToJsonString();
            Console.WriteLine(xml);
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

            var type1 = ServiceContext.ContentTypeService.GetContentType(1045);
            var type2 = ServiceContext.ContentTypeService.GetContentType(1046);

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

            var type1 = ServiceContext.ContentTypeService.GetContentType(1047);

            Assert.That(type1, Is.Not.Null);
            Assert.That(type1.PropertyGroups.Count(), Is.EqualTo(2));
            Assert.That(type1.PropertyTypes.Count(), Is.EqualTo(4));

        }

        private SerializationService SerializationService { get; set; }

        private static int[] GetTopologicalSortOrder(IList<DependencyField> fields)
        {
            var g = new TopologicalSorter(fields.Count());
            var _indexes = new Dictionary<string, int>();

            //add vertices
            for (int i = 0; i < fields.Count(); i++)
            {
                _indexes[fields[i].Alias.ToLower()] = g.AddVertex(i);
            }

            //add edges
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].DependsOn != null)
                {
                    for (int j = 0; j < fields[i].DependsOn.Length; j++)
                    {
                        g.AddEdge(i,
                            _indexes[fields[i].DependsOn[j].ToLower()]);
                    }
                }
            }

            int[] result = g.Sort();
            return result;

        }

        [TearDown]
        public override void TearDown()
        {
            DatabaseContext.Database.Dispose();

            //reset the app context
            DataTypesResolver.Reset();
            ApplicationContext.Current = null;
            Resolution.IsFrozen = false;

            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            ServiceContext = null;
            SerializationService = null;
        }
    }
}