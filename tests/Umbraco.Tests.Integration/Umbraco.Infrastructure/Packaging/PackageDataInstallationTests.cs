// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Packaging;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services.Importing;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Packaging
{
    [TestFixture]
    [Category("Slow")]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
    public class PackageDataInstallationTests : UmbracoIntegrationTestWithContent
    {
        private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

        private IMacroService MacroService => GetRequiredService<IMacroService>();

        [HideFromTypeFinder]
        [DataEditor("7e062c13-7c41-4ad9-b389-41d88aeef87c", "Editor1", "editor1")]
        public class Editor1 : DataEditor
        {
            public Editor1(IDataValueEditorFactory dataValueEditorFactory)
                : base(dataValueEditorFactory)
            {
            }
        }

        [HideFromTypeFinder]
        [DataEditor("d15e1281-e456-4b24-aa86-1dda3e4299d5", "Editor2", "editor2")]
        public class Editor2 : DataEditor
        {
            public Editor2(IDataValueEditorFactory dataValueEditorFactory)
                : base(dataValueEditorFactory)
            {
            }
        }

        //// protected override void Compose()
        //// {
        ////     base.Compose();
        ////
        ////     // the packages that are used by these tests reference totally bogus property
        ////     // editors that must exist - so they are defined here - and in order not to
        ////     // pollute everything, they are ignored by the type finder and explicitely
        ////     // added to the editors collection
        ////
        ////     Builder.WithCollectionBuilder<DataEditorCollectionBuilder>()
        ////         .Add<Editor1>()
        ////         .Add<Editor2>();
        //// }
        ////
        //// protected override void ComposeApplication(bool withApplication)
        //// {
        ////     base.ComposeApplication(withApplication);
        ////
        ////     if (!withApplication) return;
        ////
        ////     // re-register with actual media fs
        ////     Builder.ComposeFileSystems();
        //// }

        private PackageDataInstallation PackageDataInstallation => GetRequiredService<PackageDataInstallation>();

        private IMediaService MediaService => GetRequiredService<IMediaService>();

        private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

        [Test]
        public void Can_Import_uBlogsy_ContentTypes_And_Verify_Structure()
        {
            // Arrange
            string strXml = ImportResources.uBlogsy_Package;
            var xml = XElement.Parse(strXml);
            XElement dataTypeElement = xml.Descendants("DataTypes").First();
            XElement templateElement = xml.Descendants("Templates").First();
            XElement docTypeElement = xml.Descendants("DocumentTypes").First();

            // Act
            IReadOnlyList<IDataType> dataTypes = PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
            IReadOnlyList<ITemplate> templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
            IReadOnlyList<IContentType> contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);

            int numberOfTemplates = (from doc in templateElement.Elements("Template") select doc).Count();
            int numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

            // Assert
            Assert.That(dataTypes.Any(), Is.True);
            Assert.That(templates.Any(), Is.True);
            Assert.That(templates.Count(), Is.EqualTo(numberOfTemplates));
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));

            IContentType uBlogsyBaseDocType = contentTypes.First(x => x.Alias == "uBlogsyBaseDocType");
            Assert.That(uBlogsyBaseDocType.PropertyTypes.Count(), Is.EqualTo(5));
            Assert.That(uBlogsyBaseDocType.PropertyGroups.Any(), Is.False);

            IContentType uBlogsyBasePage = contentTypes.First(x => x.Alias == "uBlogsyBasePage");
            Assert.That(uBlogsyBasePage.ContentTypeCompositionExists("uBlogsyBaseDocType"), Is.True);
            Assert.That(uBlogsyBasePage.PropertyTypes.Count(), Is.EqualTo(7));
            Assert.That(uBlogsyBasePage.PropertyGroups.Count, Is.EqualTo(3));
            Assert.That(uBlogsyBasePage.PropertyGroups["content"].PropertyTypes.Count, Is.EqualTo(3));
            Assert.That(uBlogsyBasePage.PropertyGroups["sEO"].PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(uBlogsyBasePage.PropertyGroups["navigation"].PropertyTypes.Count(), Is.EqualTo(1));
            Assert.That(uBlogsyBasePage.CompositionPropertyTypes.Count(), Is.EqualTo(12));

            IContentType uBlogsyLanding = contentTypes.First(x => x.Alias == "uBlogsyLanding");
            Assert.That(uBlogsyLanding.ContentTypeCompositionExists("uBlogsyBasePage"), Is.True);
            Assert.That(uBlogsyLanding.ContentTypeCompositionExists("uBlogsyBaseDocType"), Is.True);
            Assert.That(uBlogsyLanding.PropertyTypes.Count(), Is.EqualTo(5));
            Assert.That(uBlogsyLanding.PropertyGroups.Count(), Is.EqualTo(2));
            Assert.That(uBlogsyLanding.CompositionPropertyTypes.Count(), Is.EqualTo(17));
            Assert.That(uBlogsyLanding.CompositionPropertyGroups.Count(), Is.EqualTo(5));
        }

        [Test]
        public void Can_Import_Inherited_ContentTypes_And_Verify_PropertyTypes_UniqueIds()
        {
            // Arrange
            string strXml = ImportResources.InheritedDocTypes_Package;
            var xml = XElement.Parse(strXml);
            XElement dataTypeElement = xml.Descendants("DataTypes").First();
            XElement templateElement = xml.Descendants("Templates").First();
            XElement docTypeElement = xml.Descendants("DocumentTypes").First();

            // Act
            IReadOnlyList<IDataType> dataTypes = PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
            IReadOnlyList<ITemplate> templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
            IReadOnlyList<IContentType> contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);

            // Assert
            IContentType mRBasePage = contentTypes.First(x => x.Alias == "MRBasePage");
            using IScope scope = ScopeProvider.CreateScope();
            foreach (IPropertyType propertyType in mRBasePage.PropertyTypes)
            {
                PropertyTypeDto propertyTypeDto = ScopeAccessor.AmbientScope.Database.First<PropertyTypeDto>("WHERE id = @id", new { id = propertyType.Id });
                Assert.AreEqual(propertyTypeDto.UniqueId, propertyType.Key);
            }
        }

        [Test]
        public void Can_Import_Inherited_ContentTypes_And_Verify_PropertyGroups_And_PropertyTypes()
        {
            // Arrange
            string strXml = ImportResources.InheritedDocTypes_Package;
            var xml = XElement.Parse(strXml);
            XElement dataTypeElement = xml.Descendants("DataTypes").First();
            XElement templateElement = xml.Descendants("Templates").First();
            XElement docTypeElement = xml.Descendants("DocumentTypes").First();

            // Act
            IReadOnlyList<IDataType> dataTypes = PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
            IReadOnlyList<ITemplate> templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
            IReadOnlyList<IContentType> contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);

            int numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

            // Assert
            Assert.That(dataTypes.Any(), Is.False);
            Assert.That(templates.Any(), Is.False);
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));

            IContentType mRBasePage = contentTypes.First(x => x.Alias == "MRBasePage");
            Assert.That(mRBasePage.PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(mRBasePage.PropertyGroups.Count(), Is.EqualTo(1));
            Assert.That(mRBasePage.PropertyGroups["metadaten"].PropertyTypes.Count(), Is.EqualTo(2));

            IContentType mRStartPage = contentTypes.First(x => x.Alias == "MRStartPage");
            Assert.That(mRStartPage.ContentTypeCompositionExists("MRBasePage"), Is.True);
            Assert.That(mRStartPage.PropertyTypes.Count(), Is.EqualTo(28));
            Assert.That(mRStartPage.PropertyGroups.Count(), Is.EqualTo(7));

            IEnumerable<PropertyGroup> propertyGroups = mRStartPage.CompositionPropertyGroups.Where(x => x.Name == "Metadaten");
            IEnumerable<IPropertyType> propertyTypes = propertyGroups.SelectMany(x => x.PropertyTypes);
            Assert.That(propertyGroups.Count(), Is.EqualTo(2));
            Assert.That(propertyTypes.Count(), Is.EqualTo(6));
        }

        [Test]
        public void Can_Import_Template_Package_Xml()
        {
            // Arrange
            string strXml = ImportResources.StandardMvc_Package;
            var xml = XElement.Parse(strXml);
            XElement element = xml.Descendants("Templates").First();

            int init = FileService.GetTemplates().Count();

            // Act
            IReadOnlyList<ITemplate> templates = PackageDataInstallation.ImportTemplates(element.Elements("Template").ToList(), 0);
            int numberOfTemplates = (from doc in element.Elements("Template") select doc).Count();
            IEnumerable<ITemplate> allTemplates = FileService.GetTemplates();

            // Assert
            Assert.That(templates, Is.Not.Null);
            Assert.That(templates.Any(), Is.True);
            Assert.That(templates.Count(), Is.EqualTo(numberOfTemplates));

            Assert.AreEqual(init + numberOfTemplates, allTemplates.Count());
            Assert.IsTrue(allTemplates.All(x => x.Content.Contains("UmbracoViewPage")));
        }

        [Test]
        public void Can_Import_Single_Template()
        {
            // Arrange
            string strXml = ImportResources.StandardMvc_Package;
            var xml = XElement.Parse(strXml);
            XElement element = xml.Descendants("Templates").First();

            // Act
            IEnumerable<ITemplate> templates = PackageDataInstallation.ImportTemplate(element.Elements("Template").First(), 0);

            // Assert
            Assert.That(templates, Is.Not.Null);
            Assert.That(templates.Any(), Is.True);
            Assert.That(templates.Count(), Is.EqualTo(1));

            var template = templates.First();
            Assert.AreEqual(template.Name, "Articles");
        }

        [Test]
        public void Can_Import_Single_Template_With_Key()
        {
            // Arrange
            string strXml = ImportResources.StandardMvc_Package;
            var xml = XElement.Parse(strXml);
            XElement element = xml.Descendants("Templates").First();

            var firstTemplateElement = element.Elements("Template").First();
            var key = Guid.NewGuid();
            firstTemplateElement.Add(new XElement("Key", key));

            // Act
            IEnumerable<ITemplate> templates = PackageDataInstallation.ImportTemplate(firstTemplateElement, 0);

            // Assert
            Assert.That(templates, Is.Not.Null);
            Assert.That(templates.Any(), Is.True);
            Assert.That(templates.Count(), Is.EqualTo(1));

            var template = templates.First();
            Assert.AreEqual(template.Name, "Articles");
            Assert.AreEqual(template.Key, key);
        }

        [Test]
        public void Can_Import_StandardMvc_ContentTypes_Package_Xml()
        {
            // Arrange
            string strXml = ImportResources.StandardMvc_Package;
            var xml = XElement.Parse(strXml);
            XElement dataTypeElement = xml.Descendants("DataTypes").First();
            XElement templateElement = xml.Descendants("Templates").First();
            XElement docTypeElement = xml.Descendants("DocumentTypes").First();

            // Act
            IReadOnlyList<IDataType> dataTypeDefinitions = PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
            IReadOnlyList<ITemplate> templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
            IReadOnlyList<IContentType> contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);
            int numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

            // Assert
            Assert.That(dataTypeDefinitions, Is.Not.Null);
            Assert.That(dataTypeDefinitions.Any(), Is.True);
            Assert.That(templates.Any(), Is.True);
            Assert.That(contentTypes, Is.Not.Null);
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));
            Assert.That(contentTypes.Count(x => x.ParentId == -1), Is.EqualTo(1));

            IContentType contentMaster = contentTypes.First(x => x.Alias == "ContentMaster");
            Assert.That(contentMaster.PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(contentMaster.PropertyGroups.Count(), Is.EqualTo(1));
            Assert.That(contentMaster.PropertyGroups["sEO"].PropertyTypes.Count(), Is.EqualTo(3));
            Assert.That(contentMaster.ContentTypeCompositionExists("Base"), Is.True);

            int propertyGroupId = contentMaster.PropertyGroups["sEO"].Id;
            Assert.That(contentMaster.PropertyGroups["sEO"].PropertyTypes.Any(x => x.PropertyGroupId.Value != propertyGroupId), Is.False);
        }

        [Test]
        public void Can_Import_StandardMvc_ContentTypes_And_Templates_Xml()
        {
            // Arrange
            string strXml = ImportResources.StandardMvc_Package;
            var xml = XElement.Parse(strXml);
            XElement dataTypeElement = xml.Descendants("DataTypes").First();
            XElement templateElement = xml.Descendants("Templates").First();
            XElement docTypeElement = xml.Descendants("DocumentTypes").First();

            // Act
            IReadOnlyList<IDataType> dataTypeDefinitions = PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
            IReadOnlyList<ITemplate> templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
            IReadOnlyList<IContentType> contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);
            int numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

            // Assert - Re-Import contenttypes doesn't throw
            Assert.DoesNotThrow(() => PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0));
            Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));
            Assert.That(dataTypeDefinitions, Is.Not.Null);
            Assert.That(dataTypeDefinitions.Any(), Is.True);
            Assert.That(templates.Any(), Is.True);
        }

        [Test]
        public void Can_Import_Fanoe_Starterkit_ContentTypes_And_Templates_Xml()
        {
            // Arrange
            string strXml = ImportResources.Fanoe_Package;
            var xml = XElement.Parse(strXml);
            XElement dataTypeElement = xml.Descendants("DataTypes").First();
            XElement templateElement = xml.Descendants("Templates").First();
            XElement docTypeElement = xml.Descendants("DocumentTypes").First();

            // Act
            IReadOnlyList<IDataType> dataTypeDefinitions = PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
            IReadOnlyList<ITemplate> templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
            IReadOnlyList<IContentType> contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);
            int numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

            // Assert - Re-Import contenttypes doesn't throw
            Assert.DoesNotThrow(() => PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0));
            Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));
            Assert.That(dataTypeDefinitions, Is.Not.Null);
            Assert.That(dataTypeDefinitions.Any(), Is.True);
            Assert.That(templates.Any(), Is.True);
        }

        [Test]
        public void Can_Import_Content_Package_Xml()
        {
            // Arrange
            string strXml = ImportResources.StandardMvc_Package;
            var xml = XElement.Parse(strXml);
            XElement dataTypeElement = xml.Descendants("DataTypes").First();
            XElement docTypesElement = xml.Descendants("DocumentTypes").First();
            XElement element = xml.Descendants("DocumentSet").First();
            var packageDocument = CompiledPackageContentBase.Create(element);

            // Act
            IReadOnlyList<IDataType> dataTypeDefinitions = PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
            IReadOnlyList<IContentType> contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypesElement.Elements("DocumentType"), 0);
            var importedContentTypes = contentTypes.ToDictionary(x => x.Alias, x => x);
            IReadOnlyList<IContent> contents = PackageDataInstallation.ImportContentBase(packageDocument.Yield(), importedContentTypes, 0, ContentTypeService, ContentService);
            int numberOfDocs = (from doc in element.Descendants()
                                where (string)doc.Attribute("isDoc") == string.Empty
                                select doc).Count();

            // Assert
            Assert.That(contents, Is.Not.Null);
            Assert.That(dataTypeDefinitions.Any(), Is.True);
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.EqualTo(numberOfDocs));
        }

        [Test]
        public void Can_Import_Media_Package_Xml()
        {
            // Arrange
            string strXml = ImportResources.MediaTypesAndMedia_Package_xml;
            var xml = XElement.Parse(strXml);
            XElement mediaTypesElement = xml.Descendants("MediaTypes").First();
            XElement element = xml.Descendants("MediaSet").First();
            var packageMedia = CompiledPackageContentBase.Create(element);

            // Act
            IReadOnlyList<IMediaType> mediaTypes = PackageDataInstallation.ImportMediaTypes(mediaTypesElement.Elements("MediaType"), 0);
            var importedMediaTypes = mediaTypes.ToDictionary(x => x.Alias, x => x);
            IReadOnlyList<IMedia> medias = PackageDataInstallation.ImportContentBase(packageMedia.Yield(), importedMediaTypes, 0, MediaTypeService, MediaService);
            int numberOfDocs = (from doc in element.Descendants()
                                where (string)doc.Attribute("isDoc") == string.Empty
                                select doc).Count();

            // Assert
            Assert.That(medias, Is.Not.Null);
            Assert.That(mediaTypes.Any(), Is.True);
            Assert.That(medias.Any(), Is.True);
            Assert.That(medias.Count(), Is.EqualTo(numberOfDocs));
        }

        [Test]
        public void Can_Import_CheckboxList_Content_Package_Xml_With_Property_Editor_Aliases()
            => AssertCheckBoxListTests(ImportResources.CheckboxList_Content_Package);

        private void AssertCheckBoxListTests(string strXml)
        {
            // Arrange
            var xml = XElement.Parse(strXml);
            XElement dataTypeElement = xml.Descendants("DataTypes").First();
            XElement docTypesElement = xml.Descendants("DocumentTypes").First();
            XElement element = xml.Descendants("DocumentSet").First();
            var packageDocument = CompiledPackageContentBase.Create(element);

            // Act
            IReadOnlyList<IDataType> dataTypeDefinitions = PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
            IReadOnlyList<IContentType> contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypesElement.Elements("DocumentType"), 0);
            var importedContentTypes = contentTypes.ToDictionary(x => x.Alias, x => x);
            IReadOnlyList<IContent> contents = PackageDataInstallation.ImportContentBase(packageDocument.Yield(), importedContentTypes, 0, ContentTypeService, ContentService);
            int numberOfDocs = (from doc in element.Descendants()
                                where (string)doc.Attribute("isDoc") == string.Empty
                                select doc).Count();

            string configuration;
            using (IScope scope = ScopeProvider.CreateScope())
            {
                List<DataTypeDto> dtos = ScopeAccessor.AmbientScope.Database.Fetch<DataTypeDto>("WHERE nodeId = @Id", new { dataTypeDefinitions.First().Id });
                configuration = dtos.Single().Configuration;
            }

            // Assert
            Assert.That(dataTypeDefinitions, Is.Not.Null);
            Assert.That(dataTypeDefinitions.Any(), Is.True);
            Assert.AreEqual(Constants.PropertyEditors.Aliases.CheckBoxList, dataTypeDefinitions.First().EditorAlias);
            Assert.That(contents, Is.Not.Null);
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contents.Any(), Is.True);
            Assert.That(contents.Count(), Is.EqualTo(numberOfDocs));
            Assert.AreEqual("{\"items\":[{\"id\":59,\"value\":\"test\"},{\"id\":60,\"value\":\"test3\"},{\"id\":61,\"value\":\"test2\"}]}", configuration);
        }

        [Test]
        public void Can_Import_Templates_Package_Xml_With_Invalid_Master()
        {
            // Arrange
            string strXml = ImportResources.XsltSearch_Package;
            var xml = XElement.Parse(strXml);
            XElement templateElement = xml.Descendants("Templates").First();

            // Act
            IReadOnlyList<ITemplate> templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
            int numberOfTemplates = (from doc in templateElement.Elements("Template") select doc).Count();

            // Assert
            Assert.That(templates.Any(), Is.True);
            Assert.That(templates.Count(), Is.EqualTo(numberOfTemplates));
        }

        [Test]
        public void Can_Import_Single_DocType()
        {
            // Arrange
            string strXml = ImportResources.SingleDocType;
            var docTypeElement = XElement.Parse(strXml);

            // Act
            IReadOnlyList<IContentType> contentTypes = PackageDataInstallation.ImportDocumentType(docTypeElement, 0);

            // Assert
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Any(x => x.HasIdentity == false), Is.False);
            Assert.That(contentTypes.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Can_Export_Single_DocType()
        {
            // Arrange
            string strXml = ImportResources.SingleDocType;
            var docTypeElement = XElement.Parse(strXml);

            IEntityXmlSerializer serializer = GetRequiredService<IEntityXmlSerializer>();

            // Act
            IReadOnlyList<IContentType> contentTypes = PackageDataInstallation.ImportDocumentType(docTypeElement, 0);
            IContentType contentType = contentTypes.FirstOrDefault();
            XElement element = serializer.Serialize(contentType);

            // Assert
            Assert.That(element, Is.Not.Null);
            Assert.That(element.Element("Info"), Is.Not.Null);
            Assert.That(element.Element("Structure"), Is.Not.Null);
            Assert.That(element.Element("GenericProperties"), Is.Not.Null);
            Assert.That(element.Element("Tabs"), Is.Not.Null);

            // Can't compare this XElement because the templates are not imported (they don't exist)
            //// Assert.That(XNode.DeepEquals(docTypeElement, element), Is.True);
        }

        [Test]
        public void Can_ReImport_Single_DocType()
        {
            // Arrange
            string strXml = ImportResources.SingleDocType;
            var docTypeElement = XElement.Parse(strXml);

            // Act
            IReadOnlyList<IContentType> contentTypes = PackageDataInstallation.ImportDocumentType(docTypeElement, 0);
            IReadOnlyList<IContentType> contentTypesUpdated = PackageDataInstallation.ImportDocumentType(docTypeElement, 0);

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
        public void Can_ReImport_Templates_To_Update()
        {
            var newPackageXml = XElement.Parse(ImportResources.TemplateOnly_Package);
            var updatedPackageXml = XElement.Parse(ImportResources.TemplateOnly_Updated_Package);

            XElement templateElement = newPackageXml.Descendants("Templates").First();
            XElement templateElementUpdated = updatedPackageXml.Descendants("Templates").First();

            IFileService fileService = FileService;

            // kill default test data
            fileService.DeleteTemplate("Textpage");

            // Act
            int numberOfTemplates = (from doc in templateElement.Elements("Template") select doc).Count();
            IReadOnlyList<ITemplate> templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
            IReadOnlyList<ITemplate> templatesAfterUpdate = PackageDataInstallation.ImportTemplates(templateElementUpdated.Elements("Template").ToList(), 0);
            IEnumerable<ITemplate> allTemplates = fileService.GetTemplates();

            // Assert
            Assert.That(templates.Any(), Is.True);
            Assert.That(templates.Count(), Is.EqualTo(numberOfTemplates));
            Assert.That(templatesAfterUpdate.Count(), Is.EqualTo(numberOfTemplates));
            Assert.That(allTemplates.Count(), Is.EqualTo(numberOfTemplates));
            Assert.That(allTemplates.First(x => x.Alias == "umbHomepage").Content, Contains.Substring("THIS HAS BEEN UPDATED!"));
        }

        [Test]
        public void Can_Import_DictionaryItems()
        {
            // Arrange
            const string expectedEnglishParentValue = "ParentValue";
            const string expectedNorwegianParentValue = "ForelderVerdi";
            const string expectedEnglishChildValue = "ChildValue";
            const string expectedNorwegianChildValue = "BarnVerdi";

            var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
            XElement dictionaryItemsElement = newPackageXml.Elements("DictionaryItems").First();

            AddLanguages();

            // Act
            PackageDataInstallation.ImportDictionaryItems(dictionaryItemsElement.Elements("DictionaryItem"), 0);

            // Assert
            AssertDictionaryItem("Parent", expectedEnglishParentValue, "en-GB");
            AssertDictionaryItem("Parent", expectedNorwegianParentValue, "nb-NO");
            AssertDictionaryItem("Child", expectedEnglishChildValue, "en-GB");
            AssertDictionaryItem("Child", expectedNorwegianChildValue, "nb-NO");
        }

        [Test]
        public void Can_Import_Nested_DictionaryItems()
        {
            // Arrange
            const string parentKey = "Parent";
            const string childKey = "Child";

            var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
            XElement dictionaryItemsElement = newPackageXml.Elements("DictionaryItems").First();

            AddLanguages();

            // Act
            IReadOnlyList<IDictionaryItem> dictionaryItems = PackageDataInstallation.ImportDictionaryItems(dictionaryItemsElement.Elements("DictionaryItem"), 0);

            // Assert
            Assert.That(LocalizationService.DictionaryItemExists(parentKey), "DictionaryItem parentKey does not exist");
            Assert.That(LocalizationService.DictionaryItemExists(childKey), "DictionaryItem childKey does not exist");

            IDictionaryItem parentDictionaryItem = LocalizationService.GetDictionaryItemByKey(parentKey);
            IDictionaryItem childDictionaryItem = LocalizationService.GetDictionaryItemByKey(childKey);

            Assert.That(parentDictionaryItem.ParentId, Is.Not.EqualTo(childDictionaryItem.ParentId));
            Assert.That(childDictionaryItem.ParentId, Is.EqualTo(parentDictionaryItem.Key));
        }

        [Test]
        public void WhenExistingDictionaryKey_ImportsNewChildren()
        {
            // Arrange
            const string expectedEnglishParentValue = "ExistingParentValue";
            const string expectedNorwegianParentValue = "EksisterendeForelderVerdi";
            const string expectedEnglishChildValue = "ChildValue";
            const string expectedNorwegianChildValue = "BarnVerdi";

            var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
            XElement dictionaryItemsElement = newPackageXml.Elements("DictionaryItems").First();

            AddLanguages();
            AddExistingEnglishAndNorwegianParentDictionaryItem(expectedEnglishParentValue, expectedNorwegianParentValue);

            // Act
            PackageDataInstallation.ImportDictionaryItems(dictionaryItemsElement.Elements("DictionaryItem"), 0);

            // Assert
            AssertDictionaryItem("Parent", expectedEnglishParentValue, "en-GB");
            AssertDictionaryItem("Parent", expectedNorwegianParentValue, "nb-NO");
            AssertDictionaryItem("Child", expectedEnglishChildValue, "en-GB");
            AssertDictionaryItem("Child", expectedNorwegianChildValue, "nb-NO");
        }

        [Test]
        public void WhenExistingDictionaryKey_OnlyAddsNewLanguages()
        {
            // Arrange
            const string expectedEnglishParentValue = "ExistingParentValue";
            const string expectedNorwegianParentValue = "ForelderVerdi";
            const string expectedEnglishChildValue = "ChildValue";
            const string expectedNorwegianChildValue = "BarnVerdi";

            var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
            XElement dictionaryItemsElement = newPackageXml.Elements("DictionaryItems").First();

            AddLanguages();
            AddExistingEnglishParentDictionaryItem(expectedEnglishParentValue);

            // Act
            PackageDataInstallation.ImportDictionaryItems(dictionaryItemsElement.Elements("DictionaryItem"), 0);

            // Assert
            AssertDictionaryItem("Parent", expectedEnglishParentValue, "en-GB");
            AssertDictionaryItem("Parent", expectedNorwegianParentValue, "nb-NO");
            AssertDictionaryItem("Child", expectedEnglishChildValue, "en-GB");
            AssertDictionaryItem("Child", expectedNorwegianChildValue, "nb-NO");
        }

        [Test]
        public void Can_Import_Languages()
        {
            // Arrange
            var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
            XElement languageItemsElement = newPackageXml.Elements("Languages").First();

            // Act
            IReadOnlyList<ILanguage> languages = PackageDataInstallation.ImportLanguages(languageItemsElement.Elements("Language"), 0);
            IEnumerable<ILanguage> allLanguages = LocalizationService.GetAllLanguages();

            // Assert
            Assert.That(languages.Any(x => x.HasIdentity == false), Is.False);
            foreach (ILanguage language in languages)
            {
                Assert.That(allLanguages.Any(x => x.IsoCode == language.IsoCode), Is.True);
            }
        }

        [Test]
        public void Can_Import_Macros()
        {
            // Arrange
            string strXml = ImportResources.uBlogsy_Package;
            var xml = XElement.Parse(strXml);
            XElement macrosElement = xml.Descendants("Macros").First();

            // Act
            var macros = PackageDataInstallation.ImportMacros(
                macrosElement.Elements("macro"),
                0).ToList();

            // Assert
            Assert.That(macros.Any(), Is.True);

            var allMacros = MacroService.GetAll().ToList();
            foreach (IMacro macro in macros)
            {
                Assert.That(allMacros.Any(x => x.Alias == macro.Alias), Is.True);
            }
        }

        [Test]
        public void Can_Import_Macros_With_Properties()
        {
            // Arrange
            string strXml = ImportResources.XsltSearch_Package;
            var xml = XElement.Parse(strXml);
            XElement macrosElement = xml.Descendants("Macros").First();

            // Act
            var macros = PackageDataInstallation.ImportMacros(
                macrosElement.Elements("macro"),
                0).ToList();

            // Assert
            Assert.That(macros.Any(), Is.True);
            Assert.That(macros.First().Properties.Values.Any(), Is.True);

            var allMacros = MacroService.GetAll().ToList();
            foreach (IMacro macro in macros)
            {
                Assert.That(allMacros.Any(x => x.Alias == macro.Alias), Is.True);
            }
        }

        [Test]
        public void Can_Import_Package_With_Compositions()
        {
            // Arrange
            string strXml = ImportResources.CompositionsTestPackage;
            var xml = XElement.Parse(strXml);
            XElement templateElement = xml.Descendants("Templates").First();
            XElement docTypeElement = xml.Descendants("DocumentTypes").First();

            // Act
            IReadOnlyList<ITemplate> templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
            IReadOnlyList<IContentType> contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);
            int numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

            // Assert
            Assert.That(contentTypes, Is.Not.Null);
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));
            Assert.That(contentTypes.Count(x => x.ParentId == -1), Is.EqualTo(3));

            IContentType textpage = contentTypes.First(x => x.Alias.Equals("umbTextyPage"));
            Assert.That(textpage.ParentId, Is.Not.EqualTo(-1));
            Assert.That(textpage.ContentTypeComposition.Count(), Is.EqualTo(3));
            Assert.That(textpage.ContentTypeCompositionExists("umbMaster"), Is.True);
            Assert.That(textpage.ContentTypeCompositionExists("Meta"), Is.True);
            Assert.That(textpage.ContentTypeCompositionExists("Seo"), Is.True);
        }

        [Test]
        public void Can_Import_Package_With_Compositions_Ordered()
        {
            // Arrange
            string strXml = ImportResources.CompositionsTestPackage_Random;
            var xml = XElement.Parse(strXml);
            XElement docTypeElement = xml.Descendants("DocumentTypes").First();

            // Act
            IReadOnlyList<IContentType> contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);
            int numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

            // Assert
            Assert.That(contentTypes, Is.Not.Null);
            Assert.That(contentTypes.Any(), Is.True);
            Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));

            IContentType testContentType = contentTypes.First(x => x.Alias.Equals("CompositeTest"));
            Assert.That(testContentType.ContentTypeComposition.Count(), Is.EqualTo(3));
            Assert.That(testContentType.ContentTypeCompositionExists("Content"), Is.True);
            Assert.That(testContentType.ContentTypeCompositionExists("Meta"), Is.True);
            Assert.That(testContentType.ContentTypeCompositionExists("Seo"), Is.True);
        }

        [Test]
        public void ImportDocumentType_NewTypeWithOmittedHistoryCleanupPolicy_InsertsDefaultPolicy()
        {
            // Arrange
            var withoutCleanupPolicy = XElement.Parse(ImportResources.SingleDocType);

            // Act
            var contentTypes = PackageDataInstallation
                .ImportDocumentType(withoutCleanupPolicy, 0)
                .OfType<IContentTypeWithHistoryCleanup>();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(contentTypes.Single().HistoryCleanup);
                Assert.IsFalse(contentTypes.Single().HistoryCleanup.PreventCleanup);
            });
        }

        [Test]
        public void ImportDocumentType_WithHistoryCleanupPolicyElement_ImportsWithCorrectValues()
        {
            // Arrange
            var docTypeElement = XElement.Parse(ImportResources.SingleDocType_WithCleanupPolicy);

            // Act
            var contentTypes = PackageDataInstallation
                .ImportDocumentType(docTypeElement, 0)
                .OfType<IContentTypeWithHistoryCleanup>();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(contentTypes.Single().HistoryCleanup);
                Assert.IsTrue(contentTypes.Single().HistoryCleanup.PreventCleanup);
                Assert.AreEqual(1, contentTypes.Single().HistoryCleanup.KeepAllVersionsNewerThanDays);
                Assert.AreEqual(2, contentTypes.Single().HistoryCleanup.KeepLatestVersionPerDayForDays);
            });
        }

        [Test]
        public void ImportDocumentType_ExistingTypeWithOmittedHistoryCleanupPolicy_DoesNotOverwriteDatabaseContent()
        {
            // Arrange
            var withoutCleanupPolicy = XElement.Parse(ImportResources.SingleDocType);
            var withCleanupPolicy = XElement.Parse(ImportResources.SingleDocType_WithCleanupPolicy);

            // Act
            var contentTypes = PackageDataInstallation
                .ImportDocumentType(withCleanupPolicy, 0)
                .OfType<IContentTypeWithHistoryCleanup>();

            var contentTypesUpdated = PackageDataInstallation
                .ImportDocumentType(withoutCleanupPolicy, 0)
                .OfType<IContentTypeWithHistoryCleanup>();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(contentTypes.Single().HistoryCleanup);
                Assert.IsTrue(contentTypes.Single().HistoryCleanup.PreventCleanup);
                Assert.AreEqual(1, contentTypes.Single().HistoryCleanup.KeepAllVersionsNewerThanDays);
                Assert.AreEqual(2, contentTypes.Single().HistoryCleanup.KeepLatestVersionPerDayForDays);

                Assert.NotNull(contentTypesUpdated.Single().HistoryCleanup);
                Assert.IsTrue(contentTypesUpdated.Single().HistoryCleanup.PreventCleanup);
                Assert.AreEqual(1, contentTypes.Single().HistoryCleanup.KeepAllVersionsNewerThanDays);
                Assert.AreEqual(2, contentTypes.Single().HistoryCleanup.KeepLatestVersionPerDayForDays);
            });
        }

        private void AddLanguages()
        {
            var globalSettings = new GlobalSettings();
            var norwegian = new Language(globalSettings, "nb-NO");
            var english = new Language(globalSettings, "en-GB");
            LocalizationService.Save(norwegian, 0);
            LocalizationService.Save(english, 0);
        }

        private void AssertDictionaryItem(string dictionaryItemName, string expectedValue, string cultureCode)
        {
            Assert.That(LocalizationService.DictionaryItemExists(dictionaryItemName), "DictionaryItem key does not exist");
            IDictionaryItem dictionaryItem = LocalizationService.GetDictionaryItemByKey(dictionaryItemName);
            IDictionaryTranslation translation = dictionaryItem.Translations.SingleOrDefault(i => i.Language.IsoCode == cultureCode);
            Assert.IsNotNull(translation, "Translation to {0} was not added", cultureCode);
            string value = translation.Value;
            Assert.That(value, Is.EqualTo(expectedValue), "Translation value was not set");
        }

        private void AddExistingEnglishParentDictionaryItem(string expectedEnglishParentValue)
        {
            var languages = LocalizationService.GetAllLanguages().ToList();
            ILanguage englishLanguage = languages.Single(l => l.IsoCode == "en-GB");
            LocalizationService.Save(
                new DictionaryItem("Parent")
                {
                    // This matches what is in the package.xml file
                    Key = new Guid("28f2e02a-8c66-4fcd-85e3-8524d551c0d3"),
                    Translations = new List<IDictionaryTranslation>
                                    {
                                            new DictionaryTranslation(englishLanguage, expectedEnglishParentValue),
                                    }
                });
        }

        private void AddExistingEnglishAndNorwegianParentDictionaryItem(string expectedEnglishParentValue, string expectedNorwegianParentValue)
        {
            var languages = LocalizationService.GetAllLanguages().ToList();
            ILanguage englishLanguage = languages.Single(l => l.IsoCode == "en-GB");
            ILanguage norwegianLanguage = languages.Single(l => l.IsoCode == "nb-NO");
            LocalizationService.Save(
                new DictionaryItem("Parent")
                {
                    // This matches what is in the package.xml file
                    Key = new Guid("28f2e02a-8c66-4fcd-85e3-8524d551c0d3"),
                    Translations = new List<IDictionaryTranslation>
                                    {
                                            new DictionaryTranslation(englishLanguage, expectedEnglishParentValue),
                                            new DictionaryTranslation(norwegianLanguage, expectedNorwegianParentValue),
                                    }
                });
        }
    }
}
