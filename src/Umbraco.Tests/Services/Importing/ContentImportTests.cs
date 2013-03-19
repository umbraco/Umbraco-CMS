using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Services.Importing
{
    [TestFixture, RequiresSTA]
    public class ContentImportTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void ContentTypeService_Can_Import_Package_Xml()
        {
            // Arrange
            string strXml = ImportResources.package;
            var xml = XElement.Parse(strXml);
            var element = xml.Descendants("DocumentTypes").First();
            var contentTypeService = ServiceContext.ContentTypeService;

            // Act
            var contentTypes = contentTypeService.Import(element);
            var numberOfDocTypes = (from doc in element.Elements("DocumentType") select doc).Count();

            // Assert
            Assert.That(contentTypes, Is.Not.Null);
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));
        }

        [Test]
        public void ContentService_Can_Import_Package_Xml()
        {
            // Arrange
            string strXml = ImportResources.package;
            var xml = XElement.Parse(strXml);
            var docTypesElement = xml.Descendants("DocumentTypes").First();
            var element = xml.Descendants("DocumentSet").First();
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;

            // Act
            var contentTypes = contentTypeService.Import(docTypesElement);
            var contents = contentService.Import(element);
            var numberOfDocs = (from doc in element.Descendants()
                                where (string) doc.Attribute("isDoc") == ""
                                select doc).Count();

            // Assert
            Assert.That(contents, Is.Not.Null);
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.EqualTo(numberOfDocs));
        }
    }
}