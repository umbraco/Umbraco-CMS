// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Packaging;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services.Importing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Packaging;

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
        var strXml = ImportResources.uBlogsy_Package;
        var xml = XElement.Parse(strXml);
        var dataTypeElement = xml.Descendants("DataTypes").First();
        var templateElement = xml.Descendants("Templates").First();
        var docTypeElement = xml.Descendants("DocumentTypes").First();

        // Act
        var dataTypes = PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
        var templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
        var contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);

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
        Assert.That(uBlogsyBasePage.PropertyGroups.Count, Is.EqualTo(3));
        Assert.That(uBlogsyBasePage.PropertyGroups["content"].PropertyTypes.Count, Is.EqualTo(3));
        Assert.That(uBlogsyBasePage.PropertyGroups["sEO"].PropertyTypes.Count(), Is.EqualTo(3));
        Assert.That(uBlogsyBasePage.PropertyGroups["navigation"].PropertyTypes.Count(), Is.EqualTo(1));
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
    public void Can_Import_Inherited_ContentTypes_And_Verify_PropertyTypes_UniqueIds()
    {
        // Arrange
        var strXml = ImportResources.InheritedDocTypes_Package;
        var xml = XElement.Parse(strXml);
        var dataTypeElement = xml.Descendants("DataTypes").First();
        var templateElement = xml.Descendants("Templates").First();
        var docTypeElement = xml.Descendants("DocumentTypes").First();

        // Act
        var dataTypes = PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
        var templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
        var contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);

        // Assert
        var mRBasePage = contentTypes.First(x => x.Alias == "MRBasePage");
        using var scope = ScopeProvider.CreateScope();
        foreach (var propertyType in mRBasePage.PropertyTypes)
        {
            var propertyTypeDto =
                ScopeAccessor.AmbientScope.Database.First<PropertyTypeDto>("WHERE id = @id", new { id = propertyType.Id });
            Assert.AreEqual(propertyTypeDto.UniqueId, propertyType.Key);
        }
    }

    [Test]
    public void Can_Import_Inherited_ContentTypes_And_Verify_PropertyGroups_And_PropertyTypes()
    {
        // Arrange
        var strXml = ImportResources.InheritedDocTypes_Package;
        var xml = XElement.Parse(strXml);
        var dataTypeElement = xml.Descendants("DataTypes").First();
        var templateElement = xml.Descendants("Templates").First();
        var docTypeElement = xml.Descendants("DocumentTypes").First();

        // Act
        var dataTypes = PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
        var templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
        var contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);

        var numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

        // Assert
        Assert.That(dataTypes.Any(), Is.False);
        Assert.That(templates.Any(), Is.False);
        Assert.That(contentTypes.Any(), Is.True);
        Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));

        var mRBasePage = contentTypes.First(x => x.Alias == "MRBasePage");
        Assert.That(mRBasePage.PropertyTypes.Count(), Is.EqualTo(3));
        Assert.That(mRBasePage.PropertyGroups.Count(), Is.EqualTo(1));
        Assert.That(mRBasePage.PropertyGroups["metadaten"].PropertyTypes.Count(), Is.EqualTo(2));

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
    public void Can_Import_Template_Package_Xml()
    {
        // Arrange
        var strXml = ImportResources.StandardMvc_Package;
        var xml = XElement.Parse(strXml);
        var element = xml.Descendants("Templates").First();

        var init = FileService.GetTemplates().Count();

        // Act
        var templates = PackageDataInstallation.ImportTemplates(element.Elements("Template").ToList(), 0);
        var numberOfTemplates = (from doc in element.Elements("Template") select doc).Count();
        var allTemplates = FileService.GetTemplates();

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
        var strXml = ImportResources.StandardMvc_Package;
        var xml = XElement.Parse(strXml);
        var element = xml.Descendants("Templates").First();

        // Act
        var templates = PackageDataInstallation.ImportTemplate(element.Elements("Template").First(), 0);

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
        var strXml = ImportResources.StandardMvc_Package;
        var xml = XElement.Parse(strXml);
        var element = xml.Descendants("Templates").First();

        var firstTemplateElement = element.Elements("Template").First();
        var key = Guid.NewGuid();
        firstTemplateElement.Add(new XElement("Key", key));

        // Act
        var templates = PackageDataInstallation.ImportTemplate(firstTemplateElement, 0);

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
        var strXml = ImportResources.StandardMvc_Package;
        var xml = XElement.Parse(strXml);
        var dataTypeElement = xml.Descendants("DataTypes").First();
        var templateElement = xml.Descendants("Templates").First();
        var docTypeElement = xml.Descendants("DocumentTypes").First();

        // Act
        var dataTypeDefinitions =
            PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
        var templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
        var contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);
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
        Assert.That(contentMaster.PropertyGroups["sEO"].PropertyTypes.Count(), Is.EqualTo(3));
        Assert.That(contentMaster.ContentTypeCompositionExists("Base"), Is.True);

        var propertyGroupId = contentMaster.PropertyGroups["sEO"].Id;
        Assert.That(
            contentMaster.PropertyGroups["sEO"].PropertyTypes.Any(x => x.PropertyGroupId.Value != propertyGroupId),
            Is.False);
    }

    [Test]
    public void Can_Import_StandardMvc_ContentTypes_And_Templates_Xml()
    {
        // Arrange
        var strXml = ImportResources.StandardMvc_Package;
        var xml = XElement.Parse(strXml);
        var dataTypeElement = xml.Descendants("DataTypes").First();
        var templateElement = xml.Descendants("Templates").First();
        var docTypeElement = xml.Descendants("DocumentTypes").First();

        // Act
        var dataTypeDefinitions =
            PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
        var templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
        var contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);
        var numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

        // Assert - Re-Import contenttypes doesn't throw
        Assert.DoesNotThrow(() =>
            PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0));
        Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));
        Assert.That(dataTypeDefinitions, Is.Not.Null);
        Assert.That(dataTypeDefinitions.Any(), Is.True);
        Assert.That(templates.Any(), Is.True);
    }

    [Test]
    public void Can_Import_Fanoe_Starterkit_ContentTypes_And_Templates_Xml()
    {
        // Arrange
        var strXml = ImportResources.Fanoe_Package;
        var xml = XElement.Parse(strXml);
        var dataTypeElement = xml.Descendants("DataTypes").First();
        var templateElement = xml.Descendants("Templates").First();
        var docTypeElement = xml.Descendants("DocumentTypes").First();

        // Act
        var dataTypeDefinitions =
            PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
        var templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
        var contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);
        var numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

        // Assert - Re-Import contenttypes doesn't throw
        Assert.DoesNotThrow(() =>
            PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0));
        Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));
        Assert.That(dataTypeDefinitions, Is.Not.Null);
        Assert.That(dataTypeDefinitions.Any(), Is.True);
        Assert.That(templates.Any(), Is.True);
    }

    [Test]
    public void Can_Import_Content_Package_Xml()
    {
        // Arrange
        var strXml = ImportResources.StandardMvc_Package;
        var xml = XElement.Parse(strXml);
        var dataTypeElement = xml.Descendants("DataTypes").First();
        var docTypesElement = xml.Descendants("DocumentTypes").First();
        var element = xml.Descendants("DocumentSet").First();
        var packageDocument = CompiledPackageContentBase.Create(element);

        // Act
        var dataTypeDefinitions =
            PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
        var contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypesElement.Elements("DocumentType"), 0);
        var importedContentTypes = contentTypes.ToDictionary(x => x.Alias, x => x);
        var contents = PackageDataInstallation.ImportContentBase(packageDocument.Yield(), importedContentTypes, 0, ContentTypeService, ContentService);
        var numberOfDocs = (from doc in element.Descendants()
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
        var strXml = ImportResources.MediaTypesAndMedia_Package_xml;
        var xml = XElement.Parse(strXml);
        var mediaTypesElement = xml.Descendants("MediaTypes").First();
        var element = xml.Descendants("MediaSet").First();
        var packageMedia = CompiledPackageContentBase.Create(element);

        // Act
        var mediaTypes = PackageDataInstallation.ImportMediaTypes(mediaTypesElement.Elements("MediaType"), 0);
        var importedMediaTypes = mediaTypes.ToDictionary(x => x.Alias, x => x);
        var medias = PackageDataInstallation.ImportContentBase(packageMedia.Yield(), importedMediaTypes, 0, MediaTypeService, MediaService);
        var numberOfDocs = (from doc in element.Descendants()
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
        var dataTypeElement = xml.Descendants("DataTypes").First();
        var docTypesElement = xml.Descendants("DocumentTypes").First();
        var element = xml.Descendants("DocumentSet").First();
        var packageDocument = CompiledPackageContentBase.Create(element);

        // Act
        var dataTypeDefinitions =
            PackageDataInstallation.ImportDataTypes(dataTypeElement.Elements("DataType").ToList(), 0);
        var contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypesElement.Elements("DocumentType"), 0);
        var importedContentTypes = contentTypes.ToDictionary(x => x.Alias, x => x);
        var contents = PackageDataInstallation.ImportContentBase(packageDocument.Yield(), importedContentTypes, 0, ContentTypeService, ContentService);
        var numberOfDocs = (from doc in element.Descendants()
                            where (string)doc.Attribute("isDoc") == string.Empty
                            select doc).Count();

        string configuration;
        using (var scope = ScopeProvider.CreateScope())
        {
            var dtos = ScopeAccessor.AmbientScope.Database.Fetch<DataTypeDto>("WHERE nodeId = @Id", new { dataTypeDefinitions.First().Id });
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
        Assert.AreEqual(
            "{\"items\":[{\"id\":59,\"value\":\"test\"},{\"id\":60,\"value\":\"test3\"},{\"id\":61,\"value\":\"test2\"}]}",
            configuration);
    }

    [Test]
    public void Can_Import_Templates_Package_Xml_With_Invalid_Master()
    {
        // Arrange
        var strXml = ImportResources.XsltSearch_Package;
        var xml = XElement.Parse(strXml);
        var templateElement = xml.Descendants("Templates").First();

        // Act
        var templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
        var numberOfTemplates = (from doc in templateElement.Elements("Template") select doc).Count();

        // Assert
        Assert.That(templates.Any(), Is.True);
        Assert.That(templates.Count(), Is.EqualTo(numberOfTemplates));
    }

    [Test]
    public void Can_Import_Single_DocType()
    {
        // Arrange
        var strXml = ImportResources.SingleDocType;
        var docTypeElement = XElement.Parse(strXml);

        // Act
        var contentTypes = PackageDataInstallation.ImportDocumentType(docTypeElement, 0);

        // Assert
        Assert.That(contentTypes.Any(), Is.True);
        Assert.That(contentTypes.Any(x => x.HasIdentity == false), Is.False);
        Assert.That(contentTypes.Count(), Is.EqualTo(1));
    }

    [Test]
    public void Can_Export_Single_DocType()
    {
        // Arrange
        var strXml = ImportResources.SingleDocType;
        var docTypeElement = XElement.Parse(strXml);

        var serializer = GetRequiredService<IEntityXmlSerializer>();

        // Act
        var contentTypes = PackageDataInstallation.ImportDocumentType(docTypeElement, 0);
        var contentType = contentTypes.FirstOrDefault();
        var element = serializer.Serialize(contentType);

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
        var strXml = ImportResources.SingleDocType;
        var docTypeElement = XElement.Parse(strXml);

        // Act
        var contentTypes = PackageDataInstallation.ImportDocumentType(docTypeElement, 0);
        var contentTypesUpdated = PackageDataInstallation.ImportDocumentType(docTypeElement, 0);

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

        var templateElement = newPackageXml.Descendants("Templates").First();
        var templateElementUpdated = updatedPackageXml.Descendants("Templates").First();

        var fileService = FileService;

        // kill default test data
        fileService.DeleteTemplate("Textpage");

        // Act
        var numberOfTemplates = (from doc in templateElement.Elements("Template") select doc).Count();
        var templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
        var templatesAfterUpdate =
            PackageDataInstallation.ImportTemplates(templateElementUpdated.Elements("Template").ToList(), 0);
        var allTemplates = fileService.GetTemplates();

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
        var dictionaryItemsElement = newPackageXml.Elements("DictionaryItems").First();

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
        var dictionaryItemsElement = newPackageXml.Elements("DictionaryItems").First();

        AddLanguages();

        // Act
        var dictionaryItems =
            PackageDataInstallation.ImportDictionaryItems(dictionaryItemsElement.Elements("DictionaryItem"), 0);

        // Assert
        Assert.That(LocalizationService.DictionaryItemExists(parentKey), "DictionaryItem parentKey does not exist");
        Assert.That(LocalizationService.DictionaryItemExists(childKey), "DictionaryItem childKey does not exist");

        var parentDictionaryItem = LocalizationService.GetDictionaryItemByKey(parentKey);
        var childDictionaryItem = LocalizationService.GetDictionaryItemByKey(childKey);

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
        var dictionaryItemsElement = newPackageXml.Elements("DictionaryItems").First();

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
        var dictionaryItemsElement = newPackageXml.Elements("DictionaryItems").First();

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
        var languageItemsElement = newPackageXml.Elements("Languages").First();

        // Act
        var languages = PackageDataInstallation.ImportLanguages(languageItemsElement.Elements("Language"), 0);
        var allLanguages = LocalizationService.GetAllLanguages();

        // Assert
        Assert.That(languages.Any(x => x.HasIdentity == false), Is.False);
        foreach (var language in languages)
        {
            Assert.That(allLanguages.Any(x => x.IsoCode == language.IsoCode), Is.True);
        }
    }

    [Test]
    public void Can_Import_Macros()
    {
        // Arrange
        var strXml = ImportResources.uBlogsy_Package;
        var xml = XElement.Parse(strXml);
        var macrosElement = xml.Descendants("Macros").First();

        // Act
        var macros = PackageDataInstallation.ImportMacros(
            macrosElement.Elements("macro"),
            0).ToList();

        // Assert
        Assert.That(macros.Any(), Is.True);

        var allMacros = MacroService.GetAll().ToList();
        foreach (var macro in macros)
        {
            Assert.That(allMacros.Any(x => x.Alias == macro.Alias), Is.True);
        }
    }

    [Test]
    public void Can_Import_Macros_With_Properties()
    {
        // Arrange
        var strXml = ImportResources.XsltSearch_Package;
        var xml = XElement.Parse(strXml);
        var macrosElement = xml.Descendants("Macros").First();

        // Act
        var macros = PackageDataInstallation.ImportMacros(
            macrosElement.Elements("macro"),
            0).ToList();

        // Assert
        Assert.That(macros.Any(), Is.True);
        Assert.That(macros.First().Properties.Values.Any(), Is.True);

        var allMacros = MacroService.GetAll().ToList();
        foreach (var macro in macros)
        {
            Assert.That(allMacros.Any(x => x.Alias == macro.Alias), Is.True);
        }
    }

    [Test]
    public void Can_Import_Package_With_Compositions()
    {
        // Arrange
        var strXml = ImportResources.CompositionsTestPackage;
        var xml = XElement.Parse(strXml);
        var templateElement = xml.Descendants("Templates").First();
        var docTypeElement = xml.Descendants("DocumentTypes").First();

        // Act
        var templates = PackageDataInstallation.ImportTemplates(templateElement.Elements("Template").ToList(), 0);
        var contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);
        var numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

        // Assert
        Assert.That(contentTypes, Is.Not.Null);
        Assert.That(contentTypes.Any(), Is.True);
        Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));
        Assert.That(contentTypes.Count(x => x.ParentId == -1), Is.EqualTo(3));

        var textpage = contentTypes.First(x => x.Alias.Equals("umbTextyPage"));
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
        var strXml = ImportResources.CompositionsTestPackage_Random;
        var xml = XElement.Parse(strXml);
        var docTypeElement = xml.Descendants("DocumentTypes").First();

        // Act
        var contentTypes = PackageDataInstallation.ImportDocumentTypes(docTypeElement.Elements("DocumentType"), 0);
        var numberOfDocTypes = (from doc in docTypeElement.Elements("DocumentType") select doc).Count();

        // Assert
        Assert.That(contentTypes, Is.Not.Null);
        Assert.That(contentTypes.Any(), Is.True);
        Assert.That(contentTypes.Count(), Is.EqualTo(numberOfDocTypes));

        var testContentType = contentTypes.First(x => x.Alias.Equals("CompositeTest"));
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
        var norwegian = new Language("nb-NO", "Norwegian Bokmål (Norway)");
        var english = new Language("en-GB", "English (United Kingdom)");
        LocalizationService.Save(norwegian, 0);
        LocalizationService.Save(english, 0);
    }

    private void AssertDictionaryItem(string dictionaryItemName, string expectedValue, string cultureCode)
    {
        Assert.That(LocalizationService.DictionaryItemExists(dictionaryItemName), "DictionaryItem key does not exist");
        var dictionaryItem = LocalizationService.GetDictionaryItemByKey(dictionaryItemName);
        var translation = dictionaryItem.Translations.SingleOrDefault(i => i.Language.IsoCode == cultureCode);
        Assert.IsNotNull(translation, "Translation to {0} was not added", cultureCode);
        var value = translation.Value;
        Assert.That(value, Is.EqualTo(expectedValue), "Translation value was not set");
    }

    private void AddExistingEnglishParentDictionaryItem(string expectedEnglishParentValue)
    {
        var languages = LocalizationService.GetAllLanguages().ToList();
        var englishLanguage = languages.Single(l => l.IsoCode == "en-GB");
        LocalizationService.Save(
            new DictionaryItem("Parent")
            {
                // This matches what is in the package.xml file
                Key = new Guid("28f2e02a-8c66-4fcd-85e3-8524d551c0d3"),
                Translations = new List<IDictionaryTranslation>
                {
                    new DictionaryTranslation(englishLanguage, expectedEnglishParentValue)
                }
            });
    }

    private void AddExistingEnglishAndNorwegianParentDictionaryItem(string expectedEnglishParentValue, string expectedNorwegianParentValue)
    {
        var languages = LocalizationService.GetAllLanguages().ToList();
        var englishLanguage = languages.Single(l => l.IsoCode == "en-GB");
        var norwegianLanguage = languages.Single(l => l.IsoCode == "nb-NO");
        LocalizationService.Save(
            new DictionaryItem("Parent")
            {
                // This matches what is in the package.xml file
                Key = new Guid("28f2e02a-8c66-4fcd-85e3-8524d551c0d3"),
                Translations = new List<IDictionaryTranslation>
                {
                    new DictionaryTranslation(englishLanguage, expectedEnglishParentValue),
                    new DictionaryTranslation(norwegianLanguage, expectedNorwegianParentValue)
                }
            });
    }
}
