using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Models;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using umbraco.editorControls.tinyMCE3;
using umbraco.interfaces;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.Models
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
    [TestFixture]
    public class MediaXmlTest : BaseDatabaseFactoryTest
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
        public void Can_Generate_Xml_Representation_Of_Media()
        {
            // Arrange
            var mediaType = MockedContentTypes.CreateImageMediaType("image2");
            ServiceContext.ContentTypeService.Save(mediaType);

            // reference, so static ctor runs, so event handlers register
            // and then, this will reset the width, height... because the file does not exist, of course ;-(
            var ignored = new FileUploadPropertyEditor();

            var media = MockedMedia.CreateMediaImage(mediaType, -1);
            ServiceContext.MediaService.Save(media, 0);

            // so we have to force-reset these values because the property editor has cleared them
            media.SetValue(Constants.Conventions.Media.Width, "200");
            media.SetValue(Constants.Conventions.Media.Height, "200");
            media.SetValue(Constants.Conventions.Media.Bytes, "100");
            media.SetValue(Constants.Conventions.Media.Extension, "png");

            var nodeName = media.ContentType.Alias.ToSafeAliasWithForcingCheck();
            var urlName = media.GetUrlSegment();

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
            Assert.AreEqual(media.GetCreatorProfile().Name, (string)element.Attribute("writerName"));
            Assert.AreEqual(media.CreatorId.ToString(), (string)element.Attribute("writerID"));
            Assert.AreEqual(media.Version.ToString(), (string)element.Attribute("version"));
            Assert.AreEqual("0", (string)element.Attribute("template"));

            Assert.AreEqual(media.Properties[Constants.Conventions.Media.File].Value.ToString(), element.Elements(Constants.Conventions.Media.File).Single().Value);
            Assert.AreEqual(media.Properties[Constants.Conventions.Media.Width].Value.ToString(), element.Elements(Constants.Conventions.Media.Width).Single().Value);
            Assert.AreEqual(media.Properties[Constants.Conventions.Media.Height].Value.ToString(), element.Elements(Constants.Conventions.Media.Height).Single().Value);
            Assert.AreEqual(media.Properties[Constants.Conventions.Media.Bytes].Value.ToString(), element.Elements(Constants.Conventions.Media.Bytes).Single().Value);
            Assert.AreEqual(media.Properties[Constants.Conventions.Media.Extension].Value.ToString(), element.Elements(Constants.Conventions.Media.Extension).Single().Value);
        }
    }
}