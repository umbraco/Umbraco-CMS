using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Services.Importing
{
    [TestFixture, RequiresSTA]
    public class PackageImportTests : BaseServiceTest
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
        public void PackagingService_Can_Import_uBlogsy_ContentTypes_And_Verify_Structure()
        {
            // Arrange
            string strXml = ImportResources.uBlogsy_Package;
            var xml = XElement.Parse(strXml);
            var dataTypeElement = xml.Descendants("DataTypes").First();
            var templateElement = xml.Descendants("Templates").First();
            var docTypeElement = xml.Descendants("DocumentTypes").First();
            var packagingService = ServiceContext.PackagingService;

            // Act
            var dataTypes = packagingService.ImportDataTypeDefinitions(dataTypeElement);
            var templates = packagingService.ImportTemplates(templateElement);
            var contentTypes = packagingService.ImportContentTypes(docTypeElement);

            var numberOfTemplates = (from doc in templateElement.Elements("Template") select doc).Count();
            var numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

            // Assert
            Assert.That(dataTypes.Any(), Is.True);
            Assert.That(templates.Any(), Is.True);
            Assert.That(templates.Count(), Is.EqualTo(numberOfTemplates));
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));

            var uBlogsyBaseDocType = contentTypes.First(x => x.Alias == "uBlogsyBaseDocType");
            Assert.That(uBlogsyBaseDocType.PropertyTypes.Count(), Is.EqualTo(5));
            Assert.That(uBlogsyBaseDocType.PropertyGroups.Any(), Is.False);

            var uBlogsyBasePage = contentTypes.First(x => x.Alias == "uBlogsyBasePage");
            Assert.That(uBlogsyBasePage.ContentTypeCompositionExists("uBlogsyBaseDocType"), Is.True);
            Assert.That(uBlogsyBasePage.PropertyTypes.Count(), Is.EqualTo(7));
            Assert.That(uBlogsyBasePage.PropertyGroups.Count(), Is.EqualTo(3));
            Assert.That(uBlogsyBasePage.PropertyGroups["Content"].PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(uBlogsyBasePage.PropertyGroups["SEO"].PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(uBlogsyBasePage.PropertyGroups["Navigation"].PropertyTypes.Count(), Is.EqualTo(1));
            Assert.That(uBlogsyBasePage.CompositionPropertyTypes.Count(), Is.EqualTo(12));

            var uBlogsyLanding = contentTypes.First(x => x.Alias == "uBlogsyLanding");
            Assert.That(uBlogsyLanding.ContentTypeCompositionExists("uBlogsyBasePage"), Is.True);
            Assert.That(uBlogsyLanding.ContentTypeCompositionExists("uBlogsyBaseDocType"), Is.True);
            Assert.That(uBlogsyLanding.PropertyTypes.Count(), Is.EqualTo(5));
            Assert.That(uBlogsyLanding.PropertyGroups.Count(), Is.EqualTo(2));
            Assert.That(uBlogsyLanding.CompositionPropertyTypes.Count(), Is.EqualTo(17));
            Assert.That(uBlogsyLanding.CompositionPropertyGroups.Count(), Is.EqualTo(5));
        }

        [Test]
        public void PackagingService_Can_Import_Inherited_ContentTypes_And_Verify_PropertyGroups_And_PropertyTypes()
        {
            // Arrange
            string strXml = ImportResources.InheritedDocTypes_Package;
            var xml = XElement.Parse(strXml);
            var dataTypeElement = xml.Descendants("DataTypes").First();
            var templateElement = xml.Descendants("Templates").First();
            var docTypeElement = xml.Descendants("DocumentTypes").First();
            var packagingService = ServiceContext.PackagingService;

            // Act
            var dataTypes = packagingService.ImportDataTypeDefinitions(dataTypeElement);
            var templates = packagingService.ImportTemplates(templateElement);
            var contentTypes = packagingService.ImportContentTypes(docTypeElement);

            var numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

            // Assert
            Assert.That(dataTypes.Any(), Is.False);
            Assert.That(templates.Any(), Is.False);
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));

            var mRBasePage = contentTypes.First(x => x.Alias == "MRBasePage");
            Assert.That(mRBasePage.PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(mRBasePage.PropertyGroups.Count(), Is.EqualTo(1));
            Assert.That(mRBasePage.PropertyGroups["Metadaten"].PropertyTypes.Count(), Is.EqualTo(2));

            var mRStartPage = contentTypes.First(x => x.Alias == "MRStartPage");
            Assert.That(mRStartPage.ContentTypeCompositionExists("MRBasePage"), Is.True);
            Assert.That(mRStartPage.PropertyTypes.Count(), Is.EqualTo(28));
            Assert.That(mRStartPage.PropertyGroups.Count(), Is.EqualTo(7));

            var propertyGroups = mRStartPage.CompositionPropertyGroups.Where(x => x.Name == "Metadaten");
            var propertyTypes = propertyGroups.SelectMany(x => x.PropertyTypes);
            Assert.That(propertyGroups.Count(), Is.EqualTo(2));
            Assert.That(propertyTypes.Count(), Is.EqualTo(6));
        }

        [Test]
        public void PackagingService_Can_Import_Template_Package_Xml()
        {
            // Arrange
            string strXml = ImportResources.StandardMvc_Package;
            var xml = XElement.Parse(strXml);
            var element = xml.Descendants("Templates").First();
            var packagingService = ServiceContext.PackagingService;

            // Act
            var templates = packagingService.ImportTemplates(element);
            var numberOfTemplates = (from doc in element.Elements("Template") select doc).Count();

            // Assert
            Assert.That(templates, Is.Not.Null);
            Assert.That(templates.Any(), Is.True);
            Assert.That(templates.Count(), Is.EqualTo(numberOfTemplates));
        }

        [Test]
        public void PackagingService_Can_Import_StandardMvc_ContentTypes_Package_Xml()
        {
            // Arrange
            string strXml = ImportResources.StandardMvc_Package;
            var xml = XElement.Parse(strXml);
            var dataTypeElement = xml.Descendants("DataTypes").First();
            var templateElement = xml.Descendants("Templates").First();
            var docTypeElement = xml.Descendants("DocumentTypes").First();
            var packagingService = ServiceContext.PackagingService;

            // Act
            var dataTypeDefinitions = packagingService.ImportDataTypeDefinitions(dataTypeElement);
            var templates = packagingService.ImportTemplates(templateElement);
            var contentTypes = packagingService.ImportContentTypes(docTypeElement);
            var numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

            // Assert
            Assert.That(dataTypeDefinitions, Is.Not.Null);
            Assert.That(dataTypeDefinitions.Any(), Is.True);
            Assert.That(templates.Any(), Is.True);
            Assert.That(contentTypes, Is.Not.Null);
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));
            Assert.That(contentTypes.Count(x => x.ParentId == -1), Is.EqualTo(1));

            var contentMaster = contentTypes.First(x => x.Alias == "ContentMaster");
            Assert.That(contentMaster.PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(contentMaster.PropertyGroups.Count(), Is.EqualTo(1));
            Assert.That(contentMaster.PropertyGroups["SEO"].PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(contentMaster.ContentTypeCompositionExists("Base"), Is.True);

            var propertyGroupId = contentMaster.PropertyGroups["SEO"].Id;
            Assert.That(contentMaster.PropertyGroups["SEO"].PropertyTypes.Any(x => x.PropertyGroupId.Value != propertyGroupId), Is.False);
        }

        [Test]
        public void PackagingService_Can_Import_Content_Package_Xml()
        {
            // Arrange
            string strXml = ImportResources.StandardMvc_Package;
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

        [Test]
        public void PackagingService_Can_Import_Templates_Package_Xml_With_Invalid_Master()
        {
            // Arrange
            string strXml = ImportResources.XsltSearch_Package;
            var xml = XElement.Parse(strXml);
            var templateElement = xml.Descendants("Templates").First();
            var packagingService = ServiceContext.PackagingService;

            // Act
            var templates = packagingService.ImportTemplates(templateElement);
            var numberOfTemplates = (from doc in templateElement.Elements("Template") select doc).Count();

            // Assert
            Assert.That(templates.Any(), Is.True);
            Assert.That(templates.Count(), Is.EqualTo(numberOfTemplates));
        }

        [Test]
        public void PackagingService_Can_Import_Single_DocType()
        {
            // Arrange
            string strXml = ImportResources.SingleDocType;
            var docTypeElement = XElement.Parse(strXml);
            var packagingService = ServiceContext.PackagingService;

            // Act
            var contentTypes = packagingService.ImportContentTypes(docTypeElement);

            // Assert
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Any(x => x.HasIdentity == false), Is.False);
            Assert.That(contentTypes.Count(), Is.EqualTo(1));
        }

        [Test]
        public void PackagingService_Can_ReImport_Single_DocType()
        {
            // Arrange
            string strXml = ImportResources.SingleDocType;
            var docTypeElement = XElement.Parse(strXml);

            // Act
            var contentTypes = ServiceContext.PackagingService.ImportContentTypes(docTypeElement);
            var contentTypesUpdated = ServiceContext.PackagingService.ImportContentTypes(docTypeElement);

            // Assert
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Any(x => x.HasIdentity == false), Is.False);
            Assert.That(contentTypes.Count(), Is.EqualTo(1));
            Assert.That(contentTypes.First().AllowedContentTypes.Count(), Is.EqualTo(1));

            Assert.That(contentTypesUpdated.Any(), Is.True);
            Assert.That(contentTypesUpdated.Any(x => x.HasIdentity == false), Is.False);
            Assert.That(contentTypesUpdated.Count(), Is.EqualTo(1));
            Assert.That(contentTypesUpdated.First().AllowedContentTypes.Count(), Is.EqualTo(1));
        }

        [Test]
        public void PackagingService_Can_ReImport_Templates_To_Update()
        {
            var newPackageXml = XElement.Parse(ImportResources.TemplateOnly_Package);
            var updatedPackageXml = XElement.Parse(ImportResources.TemplateOnly_Updated_Package);

            var templateElement = newPackageXml.Descendants("Templates").First();
            var templateElementUpdated = updatedPackageXml.Descendants("Templates").First();
            var packagingService = ServiceContext.PackagingService;
            var fileService = ServiceContext.FileService;

            // Act
            var numberOfTemplates = (from doc in templateElement.Elements("Template") select doc).Count();
            var templates = packagingService.ImportTemplates(templateElement);
            var templatesAfterUpdate = packagingService.ImportTemplates(templateElementUpdated);
            var allTemplates = fileService.GetTemplates();
            
            // Assert
            Assert.That(templates.Any(), Is.True);
            Assert.That(templates.Count(), Is.EqualTo(numberOfTemplates));
            Assert.That(templatesAfterUpdate.Count(), Is.EqualTo(numberOfTemplates));
            Assert.That(allTemplates.Count(), Is.EqualTo(numberOfTemplates));
            Assert.That(allTemplates.First(x => x.Alias == "umbHomepage").Content, Contains.Substring("THIS HAS BEEN UPDATED!"));
        }
    }
}