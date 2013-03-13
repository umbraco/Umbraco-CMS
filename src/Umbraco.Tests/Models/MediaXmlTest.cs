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

namespace Umbraco.Tests.Models
{
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
            UrlSegmentProviderResolver.Current = new UrlSegmentProviderResolver(typeof(DefaultUrlSegmentProvider));
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
            var mediaType = MockedContentTypes.CreateImageMediaType();
            ServiceContext.ContentTypeService.Save(mediaType);

            var media = MockedMedia.CreateMediaImage(mediaType, -1);
            ServiceContext.MediaService.Save(media, 0);

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

            Assert.AreEqual(media.Properties["umbracoFile"].Value.ToString(), element.Elements("umbracoFile").Single().Value);
            Assert.AreEqual(media.Properties["umbracoWidth"].Value.ToString(), element.Elements("umbracoWidth").Single().Value);
            Assert.AreEqual(media.Properties["umbracoHeight"].Value.ToString(), element.Elements("umbracoHeight").Single().Value);
            Assert.AreEqual(media.Properties["umbracoBytes"].Value.ToString(), element.Elements("umbracoBytes").Single().Value);
            Assert.AreEqual(media.Properties["umbracoExtension"].Value.ToString(), element.Elements("umbracoExtension").Single().Value);
        }
    }
}