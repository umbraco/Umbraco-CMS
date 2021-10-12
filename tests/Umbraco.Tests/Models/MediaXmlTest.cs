using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Extensions;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Constants = Umbraco.Cms.Core.Constants;

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
            var loggerFactory = NullLoggerFactory.Instance;
            var scheme = Mock.Of<IMediaPathScheme>();
            var contentSettings = new ContentSettings();

            var mediaFileManager = new MediaFileManager(Mock.Of<IFileSystem>(), scheme,
                loggerFactory.CreateLogger<MediaFileManager>(), ShortStringHelper);
            var ignored = new FileUploadPropertyEditor(DataValueEditorFactory, mediaFileManager, Microsoft.Extensions.Options.Options.Create(contentSettings), DataTypeService, LocalizationService, LocalizedTextService, UploadAutoFillProperties, ContentService);

            var media = MockedMedia.CreateMediaImage(mediaType, -1);
            media.WriterId = -1; // else it's zero and that's not a user and it breaks the tests
            ServiceContext.MediaService.Save(media, Constants.Security.SuperUserId);

            // so we have to force-reset these values because the property editor has cleared them
            media.SetValue(Constants.Conventions.Media.Width, "200");
            media.SetValue(Constants.Conventions.Media.Height, "200");
            media.SetValue(Constants.Conventions.Media.Bytes, "100");
            media.SetValue(Constants.Conventions.Media.Extension, "png");

            var nodeName = media.ContentType.Alias.ToSafeAlias(ShortStringHelper);
            var urlName = media.GetUrlSegment(ShortStringHelper, new[] { new DefaultUrlSegmentProvider(ShortStringHelper) });

            // Act
            XElement element = media.ToXml(Factory.GetRequiredService<IEntityXmlSerializer>());

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
