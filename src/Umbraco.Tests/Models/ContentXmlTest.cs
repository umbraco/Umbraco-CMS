using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class ContentXmlTest : TestWithDatabaseBase
    {
        [Test]
        public void Can_Generate_Xml_Representation_Of_Content()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
            ServiceContext.ContentTypeService.Save(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Root Home", -1);
            ServiceContext.ContentService.Save(content, Constants.Security.SuperUserId);

            var nodeName = content.ContentType.Alias.ToSafeAliasWithForcingCheck();
            var urlName = content.GetUrlSegment(new[]{new DefaultUrlSegmentProvider() });

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
            Assert.AreEqual(content.GetCreatorProfile(ServiceContext.UserService).Name, (string)element.Attribute("creatorName"));
            Assert.AreEqual(content.GetWriterProfile(ServiceContext.UserService).Name, (string)element.Attribute("writerName"));
            Assert.AreEqual(content.WriterId.ToString(), (string)element.Attribute("writerID"));
            Assert.AreEqual(content.TemplateId.ToString(), (string)element.Attribute("template"));

            Assert.AreEqual(content.Properties["title"].GetValue().ToString(), element.Elements("title").Single().Value);
            Assert.AreEqual(content.Properties["bodyText"].GetValue().ToString(), element.Elements("bodyText").Single().Value);
            Assert.AreEqual(content.Properties["keywords"].GetValue().ToString(), element.Elements("keywords").Single().Value);
            Assert.AreEqual(content.Properties["description"].GetValue().ToString(), element.Elements("description").Single().Value);
        }
    }
}
