// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services.Importing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class EntityXmlSerializerTests : UmbracoIntegrationTest
{
    private IEntityXmlSerializer Serializer => GetRequiredService<IEntityXmlSerializer>();
    private IContentService ContentService => GetRequiredService<IContentService>();
    private IMediaService MediaService => GetRequiredService<IMediaService>();
    private IUserService UserService => GetRequiredService<IUserService>();
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
    private IDataValueEditorFactory DataValueEditorFactory => GetRequiredService<IDataValueEditorFactory>();
    private ILocalizedTextService TextService => GetRequiredService<ILocalizedTextService>();
    private IFileService FileService => GetRequiredService<IFileService>();

    [Test]
    public void Can_Export_Macro()
    {
        // Arrange
        var macroService = GetRequiredService<IMacroService>();
        var macro = new MacroBuilder()
            .WithAlias("test1")
            .WithName("Test")
            .Build();
        macroService.Save(macro);

        // Act
        var element = Serializer.Serialize(macro);

        // Assert
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Element("name").Value, Is.EqualTo("Test"));
        Assert.That(element.Element("alias").Value, Is.EqualTo("test1"));
        Debug.Print(element.ToString());
    }

    [Test]
    public void Can_Export_DictionaryItems()
    {
        // Arrange
        CreateDictionaryData();
        var localizationService = GetRequiredService<ILocalizationService>();
        var dictionaryItem = localizationService.GetDictionaryItemByKey("Parent");

        var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
        var dictionaryItemsElement = newPackageXml.Elements("DictionaryItems").First();

        // Act
        var xml = Serializer.Serialize(new[] { dictionaryItem });

        // Assert
        Assert.That(xml.ToString(), Is.EqualTo(dictionaryItemsElement.ToString()));
    }

    [Test]
    public void Can_Export_Languages()
    {
        // Arrange
        var localizationService = GetRequiredService<ILocalizationService>();

        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .WithCultureName("Norwegian Bokmål (Norway)")
            .Build();
        localizationService.Save(languageNbNo);

        var languageEnGb = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .Build();
        localizationService.Save(languageEnGb);

        var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
        var languageItemsElement = newPackageXml.Elements("Languages").First();

        // Act
        var xml = Serializer.Serialize(new[] { languageNbNo, languageEnGb });

        // Assert
        Assert.That(xml.ToString(), Is.EqualTo(languageItemsElement.ToString()));
    }

    [Test]
    public void Can_Generate_Xml_Representation_Of_Content()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template); // else, FK violation on contentType!
        var contentType = ContentTypeBuilder.CreateTextPageContentType(
            defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Root Home", -1);
        ContentService.Save(content, Constants.Security.SuperUserId);

        var nodeName = content.ContentType.Alias.ToSafeAlias(ShortStringHelper);
        var urlName =
            content.GetUrlSegment(ShortStringHelper, new[] { new DefaultUrlSegmentProvider(ShortStringHelper) });

        // Act
        var element = content.ToXml(Serializer);

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
        Assert.AreEqual(content.GetCreatorProfile(UserService).Name, (string)element.Attribute("creatorName"));
        Assert.AreEqual(content.GetWriterProfile(UserService).Name, (string)element.Attribute("writerName"));
        Assert.AreEqual(content.WriterId.ToString(), (string)element.Attribute("writerID"));
        Assert.AreEqual(content.TemplateId.ToString(), (string)element.Attribute("template"));

        Assert.AreEqual(content.Properties["title"].GetValue().ToString(), element.Elements("title").Single().Value);
        Assert.AreEqual(content.Properties["bodyText"].GetValue().ToString(),
            element.Elements("bodyText").Single().Value);
        Assert.AreEqual(content.Properties["keywords"].GetValue().ToString(),
            element.Elements("keywords").Single().Value);
        Assert.AreEqual(content.Properties["description"].GetValue().ToString(),
            element.Elements("description").Single().Value);
    }

    [Test]
    public void Can_Generate_Xml_Representation_Of_Media()
    {
        // Arrange
        var mediaType = MediaTypeBuilder.CreateImageMediaType("image2");

        MediaTypeService.Save(mediaType);

        // reference, so static ctor runs, so event handlers register
        // and then, this will reset the width, height... because the file does not exist, of course ;-(
        var loggerFactory = NullLoggerFactory.Instance;
        var scheme = Mock.Of<IMediaPathScheme>();
        var contentSettings = new ContentSettings();

        var mediaFileManager = new MediaFileManager(
            Mock.Of<IFileSystem>(),
            scheme,
            loggerFactory.CreateLogger<MediaFileManager>(),
            ShortStringHelper,
            Services,
            Options.Create(new ContentSettings()));

        var ignored = new FileUploadPropertyEditor(
            DataValueEditorFactory,
            mediaFileManager,
            Mock.Of<IOptionsMonitor<ContentSettings>>(x => x.CurrentValue == contentSettings),
            TextService,
            Services.GetRequiredService<UploadAutoFillProperties>(),
            ContentService,
            IOHelper);

        var media = MediaBuilder.CreateMediaImage(mediaType, -1);
        media.WriterId = -1; // else it's zero and that's not a user and it breaks the tests
        MediaService.Save(media);

        // so we have to force-reset these values because the property editor has cleared them
        media.SetValue(Constants.Conventions.Media.Width, "200");
        media.SetValue(Constants.Conventions.Media.Height, "200");
        media.SetValue(Constants.Conventions.Media.Bytes, "100");
        media.SetValue(Constants.Conventions.Media.Extension, "png");

        var nodeName = media.ContentType.Alias.ToSafeAlias(ShortStringHelper);
        var urlName = media.GetUrlSegment(ShortStringHelper, new[] { new DefaultUrlSegmentProvider(ShortStringHelper) });

        // Act
        var element = media.ToXml(Serializer);

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
        Assert.AreEqual(media.GetCreatorProfile(UserService).Name, (string)element.Attribute("writerName"));
        Assert.AreEqual(media.CreatorId.ToString(), (string)element.Attribute("writerID"));
        Assert.IsNull(element.Attribute("template"));

        Assert.AreEqual(media.Properties[Constants.Conventions.Media.File].GetValue().ToString(),
            element.Elements(Constants.Conventions.Media.File).Single().Value);
        Assert.AreEqual(media.Properties[Constants.Conventions.Media.Width].GetValue().ToString(),
            element.Elements(Constants.Conventions.Media.Width).Single().Value);
        Assert.AreEqual(media.Properties[Constants.Conventions.Media.Height].GetValue().ToString(),
            element.Elements(Constants.Conventions.Media.Height).Single().Value);
        Assert.AreEqual(media.Properties[Constants.Conventions.Media.Bytes].GetValue().ToString(),
            element.Elements(Constants.Conventions.Media.Bytes).Single().Value);
        Assert.AreEqual(media.Properties[Constants.Conventions.Media.Extension].GetValue().ToString(),
            element.Elements(Constants.Conventions.Media.Extension).Single().Value);
    }

    [Test]
    public void Serialize_ForContentTypeWithHistoryCleanupPolicy_OutputsSerializedHistoryCleanupPolicy()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template); // else, FK violation on contentType!

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);

        contentType.HistoryCleanup = new HistoryCleanup
        {
            PreventCleanup = true,
            KeepAllVersionsNewerThanDays = 1,
            KeepLatestVersionPerDayForDays = 2
        };

        ContentTypeService.Save(contentType);

        // Act
        var element = Serializer.Serialize(contentType);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(element.Element("HistoryCleanupPolicy")!.Attribute("preventCleanup")!.Value,
                Is.EqualTo("true"));
            Assert.That(element.Element("HistoryCleanupPolicy")!.Attribute("keepAllVersionsNewerThanDays")!.Value,
                Is.EqualTo("1"));
            Assert.That(element.Element("HistoryCleanupPolicy")!.Attribute("keepLatestVersionPerDayForDays")!.Value,
                Is.EqualTo("2"));
        });
    }

    [Test]
    public void Serialize_ForContentTypeWithNullHistoryCleanupPolicy_DoesNotOutputSerializedDefaultPolicy()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template); // else, FK violation on contentType!

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);

        contentType.HistoryCleanup = null;

        ContentTypeService.Save(contentType);

        var element = Serializer.Serialize(contentType);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(element.Element("HistoryCleanupPolicy"), Is.Null);
        });
    }

    private void CreateDictionaryData()
    {
        var localizationService = GetRequiredService<ILocalizationService>();

        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .WithCultureName("Norwegian")
            .Build();
        localizationService.Save(languageNbNo);

        var languageEnGb = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .Build();
        localizationService.Save(languageEnGb);

        var parentItem = new DictionaryItem("Parent") { Key = Guid.Parse("28f2e02a-8c66-4fcd-85e3-8524d551c0d3") };
        var parentTranslations = new List<IDictionaryTranslation>
        {
            new DictionaryTranslation(languageNbNo, "ForelderVerdi"),
            new DictionaryTranslation(languageEnGb, "ParentValue")
        };
        parentItem.Translations = parentTranslations;
        localizationService.Save(parentItem);

        var childItem =
            new DictionaryItem(parentItem.Key, "Child") { Key = Guid.Parse("e7dba0a9-d517-4ba4-8e18-2764d392c611") };
        var childTranslations = new List<IDictionaryTranslation>
        {
            new DictionaryTranslation(languageNbNo, "BarnVerdi"),
            new DictionaryTranslation(languageEnGb, "ChildValue")
        };
        childItem.Translations = childTranslations;
        localizationService.Save(childItem);
    }
}
