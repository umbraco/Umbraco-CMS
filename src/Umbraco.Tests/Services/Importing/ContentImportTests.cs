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
            var dataTypeElement = xml.Descendants("DataTypes").First();
            var packagingService = ServiceContext.PackagingService;

            // Act
            var dataTypeDefinitions = packagingService.ImportDataTypeDefinitions(dataTypeElement);
            var contentTypes = packagingService.ImportContentTypes(element);
            var numberOfDocTypes = (from doc in element.Elements("DocumentType") select doc).Count();

            // Assert
            Assert.That(dataTypeDefinitions, Is.Not.Null);
            Assert.That(dataTypeDefinitions.Any(), Is.True);
            Assert.That(contentTypes, Is.Not.Null);
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));
            Assert.That(contentTypes.Count(x => x.ParentId == -1), Is.EqualTo(1));
        }

        [Test]
        public void ContentService_Can_Import_Package_Xml()
        {
            // Arrange
            string strXml = ImportResources.package;
            var xml = XElement.Parse(strXml);
            var dataTypeElement = xml.Descendants("DataTypes").First();
            var docTypesElement = xml.Descendants("DocumentTypes").First();
            var element = xml.Descendants("DocumentSet").First();
            var packagingService = ServiceContext.PackagingService;

            // Act
            var dataTypeDefinitions = packagingService.ImportDataTypeDefinitions(dataTypeElement);
            var contentTypes = packagingService.ImportContentTypes(docTypesElement);
            var contents = packagingService.ImportContent(element);
            var numberOfDocs = (from doc in element.Descendants()
                                where (string) doc.Attribute("isDoc") == ""
                                select doc).Count();

            // Assert
            Assert.That(contents, Is.Not.Null);
            Assert.That(dataTypeDefinitions.Any(), Is.True);
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.EqualTo(numberOfDocs));
        }
    }
}