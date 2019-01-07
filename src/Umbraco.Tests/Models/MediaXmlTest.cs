using System.Linq;
using System.Xml.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class MediaXmlTest : TestWithDatabaseBase
    {
        [Test]
        public void Can_Generate_Xml_Representation_Of_Media()
        {
            // Arrange
            var mediaType = MockedContentTypes.CreateImageMediaType("image2");
            ServiceContext.MediaTypeService.Save(mediaType);

            // reference, so static ctor runs, so event handlers register
            // and then, this will reset the width, height... because the file does not exist, of course ;-(
            var logger = Mock.Of<ILogger>();
            var scheme = Mock.Of<IMediaPathScheme>();
            var config = Mock.Of<IContentSection>();

            var mediaFileSystem = new MediaFileSystem(Mock.Of<IFileSystem>(), config, scheme, logger);
            var ignored = new FileUploadPropertyEditor(Mock.Of<ILogger>(), mediaFileSystem, config);

            var media = MockedMedia.CreateMediaImage(mediaType, -1);
            media.WriterId = -1; // else it's zero and that's not a user and it breaks the tests
            ServiceContext.MediaService.Save(media, Constants.Security.SuperUserId);

            // so we have to force-reset these values because the property editor has cleared them
            media.SetValue(Constants.Conventions.Media.Width, "200");
            media.SetValue(Constants.Conventions.Media.Height, "200");
            media.SetValue(Constants.Conventions.Media.Bytes, "100");
            media.SetValue(Constants.Conventions.Media.Extension, "png");

            var nodeName = media.ContentType.Alias.ToSafeAliasWithForcingCheck();
            var urlName = media.GetUrlSegment(new[] { new DefaultUrlSegmentProvider() });

            // Act
            XElement element = media.ToXml();

            // Assert
            Assert.That(element, Is.Not.Null);
            Assert.That(element.Name.LocalName, Is.EqualTo(nodeName));
            Assert.AreEqual(media.Id.ToString(), (string)element.Attribute("id"));
            Assert.AreEqual(media.ParentId.ToString(), (string)element.Attribute("parentID"));
            Assert.AreEqual(media.Level.ToString(), (string)element.Attribute("level"));
            Assert.AreEqual(media.SortOrder.ToString(), (string)element.Attribute("sortOrder"));
            Assert.AreEqual(media.CreateDate.ToString("s"), (string)element.Attribute("createDate"));
            Assert.AreEqual(media.UpdateDate.ToString("s"), (string)element.Attribute("updateDate"));
            Assert.AreEqual(media.Name, (string)element.Attribute("nodeName"));
            Assert.AreEqual(urlName, (string)element.Attribute("urlName"));
            Assert.AreEqual(media.Path, (string)element.Attribute("path"));
            Assert.AreEqual("", (string)element.Attribute("isDoc"));
            Assert.AreEqual(media.ContentType.Id.ToString(), (string)element.Attribute("nodeType"));
            Assert.AreEqual(media.GetCreatorProfile(ServiceContext.UserService).Name, (string)element.Attribute("writerName"));
            Assert.AreEqual(media.CreatorId.ToString(), (string)element.Attribute("writerID"));
            Assert.IsNull(element.Attribute("template"));

            Assert.AreEqual(media.Properties[Constants.Conventions.Media.File].GetValue().ToString(), element.Elements(Constants.Conventions.Media.File).Single().Value);
            Assert.AreEqual(media.Properties[Constants.Conventions.Media.Width].GetValue().ToString(), element.Elements(Constants.Conventions.Media.Width).Single().Value);
            Assert.AreEqual(media.Properties[Constants.Conventions.Media.Height].GetValue().ToString(), element.Elements(Constants.Conventions.Media.Height).Single().Value);
            Assert.AreEqual(media.Properties[Constants.Conventions.Media.Bytes].GetValue().ToString(), element.Elements(Constants.Conventions.Media.Bytes).Single().Value);
            Assert.AreEqual(media.Properties[Constants.Conventions.Media.Extension].GetValue().ToString(), element.Elements(Constants.Conventions.Media.Extension).Single().Value);
        }
    }
}
