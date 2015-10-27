using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Models
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
    [TestFixture]
    public class ContentXmlTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {                                    
            base.Initialize();
        }

        protected override void FreezeResolution()
        {
            UrlSegmentProviderResolver.Current = new UrlSegmentProviderResolver(
                new ActivatorServiceProvider(), 
                Logger,
                typeof(DefaultUrlSegmentProvider));
            base.FreezeResolution();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }
        [Test]
        public void Can_Generate_Xml_Representation_Of_Content()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            ServiceContext.ContentTypeService.Save(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Root Home", -1);
            ServiceContext.ContentService.Save(content, 0);

            var nodeName = content.ContentType.Alias.ToSafeAliasWithForcingCheck();
            var urlName = content.GetUrlSegment();

            // Act
            XElement element = content.ToXml();

            // Assert
            Assert.That(element, Is.Not.Null);
            Assert.That(element.Name.LocalName, Is.EqualTo(nodeName));
            Assert.AreEqual(content.Id.ToString(), (string)element.Attribute("id"));
            Assert.AreEqual(content.ParentId.ToString(), (string)element.Attribute("parentID"));
            Assert.AreEqual(content.Level.ToString(), (string)element.Attribute("level"));
            Assert.AreEqual(content.CreatorId.ToString(), (string)element.Attribute("creatorID"));
            Assert.AreEqual(content.SortOrder.ToString(), (string)element.Attribute("sortOrder"));
            Assert.AreEqual(content.CreateDate.ToString("s"), (string)element.Attribute("createDate"));
            Assert.AreEqual(content.UpdateDate.ToString("s"), (string)element.Attribute("updateDate"));
            Assert.AreEqual(content.Name, (string)element.Attribute("nodeName"));
            Assert.AreEqual(urlName, (string)element.Attribute("urlName"));
            Assert.AreEqual(content.Path, (string)element.Attribute("path"));
            Assert.AreEqual("", (string)element.Attribute("isDoc"));
            Assert.AreEqual(content.ContentType.Id.ToString(), (string)element.Attribute("nodeType"));
            Assert.AreEqual(content.GetCreatorProfile().Name, (string)element.Attribute("creatorName"));
            Assert.AreEqual(content.GetWriterProfile().Name, (string)element.Attribute("writerName"));
            Assert.AreEqual(content.WriterId.ToString(), (string)element.Attribute("writerID"));
            Assert.AreEqual(content.Template == null ? "0" : content.Template.Id.ToString(), (string)element.Attribute("template"));

            Assert.AreEqual(content.Properties["title"].Value.ToString(), element.Elements("title").Single().Value);
            Assert.AreEqual(content.Properties["bodyText"].Value.ToString(), element.Elements("bodyText").Single().Value);
            Assert.AreEqual(content.Properties["keywords"].Value.ToString(), element.Elements("keywords").Single().Value);
            Assert.AreEqual(content.Properties["description"].Value.ToString(), element.Elements("description").Single().Value);
        } 
    }
}