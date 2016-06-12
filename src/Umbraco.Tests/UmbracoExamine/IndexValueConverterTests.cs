using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Examine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using umbraco.interfaces;
using umbraco.presentation;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.PropertyEditors;
using UmbracoExamine;
using UmbracoExamine.DataServices;
using IContentService = UmbracoExamine.DataServices.IContentService;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public class IndexValueConverterTests : BaseDatabaseFactoryTest
    {
        [Test]
        public void Index_Delegates_To_Property_Value_Converters()
        {
            var content = new Mock<IContent>();
            long total;
            Mock.Get(serviceContext.ContentService)
                .Setup(c => c.GetPagedDescendants(-1, 0, 1000, out total, "Path", Direction.Ascending, ""))
                .Returns(new List<IContent> { content.Object });

            content.Setup(c => c.ContentType).Returns(new ContentType(-1) { Alias = "someType" });
            content.Setup(c => c.Id).Returns(1);
            content.Setup(c => c.Name).Returns("Some doc");
            content.Setup(c => c.Path).Returns("-1 1");
            content.Setup(c => c.Properties).Returns(new PropertyCollection {
                new Property(new PropertyType("textstring", DataTypeDatabaseType.Nvarchar) { Alias = "a-string"} )
                {
                    Value = "a string"
                },
                new Property(new PropertyType("indexer-test", DataTypeDatabaseType.Ntext) { Alias = "indexer-test" })
                {
                    Value = @"{
  ""name"": ""1 column layout"",
  ""sections"": [
    {
      ""grid"": 12,
      ""rows"": [
        {
          ""name"": ""Headline"",
          ""areas"": [
            {
              ""grid"": 12,
              ""hasConfig"": false,
              ""controls"": [
                {
                  ""value"": ""Hello world!"",
                  ""editor"": {
                    ""alias"": ""headline""
                  },
                  ""active"": false
                },
                {
                ""value"": ""<p>Here's the first article</p>"",
                  ""editor"": {
                    ""alias"": ""rte""
                  },
                  ""active"": false
                },
                {
                ""value"": ""<p>And here's some more RTE content</p>"",
                  ""editor"": {
                    ""alias"": ""rte""
                  },
                  ""active"": true
                }
              ],
              ""active"": true,
              ""hasActiveChild"": true
            }
          ],
          ""hasConfig"": false,
          ""id"": ""f49faed0-0df4-4ddc-0b41-c841d07b0889"",
          ""hasActiveChild"": true,
          ""active"": true
        }
      ]
    }
  ]
}"
            } });

            _indexer.IndexAll(IndexTypes.Content);

            var reader = _indexer.GetIndexWriter().GetReader();
            var allTerms = reader.Terms();
            while (allTerms.Next())
                Console.WriteLine(allTerms.Term().Field() + ": " + allTerms.Term().Text());

            var rteDocs = reader.TermDocs(new Term("indexerTest.rte"));

            var count = 0;
            while (rteDocs.Next())
                count++;

            Assert.AreEqual(2, count);
        }

        private static UmbracoExamineSearcher _searcher;
        private static UmbracoContentIndexer _indexer;
        private RAMDirectory _luceneDir;
        private ServiceContext serviceContext;

        [SetUp]
        public override void Initialize()
        {
            UmbracoExamineSearcher.DisableInitializationCheck = true;
            BaseUmbracoIndexer.DisableInitializationCheck = true;
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(new DefaultShortStringHelper(SettingsForTests.GetDefault()));

            base.Initialize();

            GetUmbracoContext("http://localhost", -1, null, true);

            _luceneDir = new RAMDirectory();
            var dataService = Mock.Of<IDataService>();
            var contentService = Mock.Of<IContentService>();
            Mock.Get(dataService).Setup(s => s.ContentService).Returns(contentService);
            Mock.Get(dataService).Setup(s => s.LogService).Returns(Mock.Of<ILogService>());
            Mock.Get(contentService).Setup(s => s.GetAllUserPropertyNames()).Returns(new[] { "aString", "indexerTest.headline", "indexerTest.rte" });
            _indexer = IndexInitializer.GetUmbracoIndexer(_luceneDir,
                dataService: dataService,
                contentService: serviceContext.ContentService, mediaService: Mock.Of<Umbraco.Core.Services.IMediaService>());
            _indexer.Initialize("indexer", new NameValueCollection { { "supportUnpublished", "true" }, { "runAsync", "false" } });


            _indexer.RebuildIndex();
            _searcher = IndexInitializer.GetUmbracoSearcher(_luceneDir);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            _luceneDir.Dispose();
            UmbracoExamineSearcher.DisableInitializationCheck = null;
            BaseUmbracoIndexer.DisableInitializationCheck = null;
        }

        protected override ApplicationContext CreateApplicationContext()
        {
            serviceContext = MockHelper.GetMockedServiceContext();

            return new ApplicationContext(
                new DatabaseContext(Mock.Of<IDatabaseFactory>(), Mock.Of<ILogger>(), Mock.Of<ISqlSyntaxProvider>(), ""),
                serviceContext,
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())
                );
        }

        protected override string GetXmlContent(int templateId)
        {
            return TestFiles.umbraco;
        }
    }

    [PropertyEditor("indexer-test", "Test indexing", "indexer-test", HideLabel = true, IsParameterEditor = false, ValueType = PropertyEditorValueTypes.Json, Group = "rich content", Icon = "icon-layout")]
    public class TestGridPropertyEditor : PropertyEditor
    {
        protected override PropertyValueEditor CreateValueEditor()
        {
            return new TestGridPropertyValueEditor();
        }
    }

    public class TestGridPropertyValueEditor : PropertyValueEditor
    {
        public override IEnumerable<XElement> ConvertDbToExamine(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            var nodeName = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "data" : property.Alias.ToSafeAlias();
            var value = property.Value as string;
            if (String.IsNullOrWhiteSpace(value))
                yield return null;
            else
            {
                var json = JsonConvert.DeserializeObject<JObject>(value);
                //section.row.area.control
                foreach (var section in json["sections"])
                {
                    foreach (var row in section["rows"])
                    {
                        foreach (var area in row["areas"])
                        {
                            foreach (var control in area["controls"])
                            {
                                var controlValue = control["value"].Value<string>();
                                var editorName = control["editor"]["alias"].Value<string>();
                                if (!String.IsNullOrWhiteSpace(controlValue))
                                    yield return new XElement(nodeName + "." + editorName, controlValue.StripHtml());
                            }
                        }
                    }
                }
            }
        }
    }

}
