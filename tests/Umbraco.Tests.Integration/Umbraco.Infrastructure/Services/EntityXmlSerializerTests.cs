// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
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

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class EntityXmlSerializerTests : UmbracoIntegrationTest
{
    private IEntityXmlSerializer Serializer => GetRequiredService<IEntityXmlSerializer>();
    private IContentService ContentService => GetRequiredService<IContentService>();
    private IMediaService MediaService => GetRequiredService<IMediaService>();
    private IUserService UserService => GetRequiredService<IUserService>();
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
    private IDataValueEditorFactory DataValueEditorFactory => GetRequiredService<IDataValueEditorFactory>();
    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    [Test]
    public async Task Can_Export_DictionaryItems()
    {
        // Arrange
        await CreateDictionaryData();
        var dictionaryItemService = GetRequiredService<IDictionaryItemService>();
        var dictionaryItem = await dictionaryItemService.GetAsync("Parent");

        var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
        var dictionaryItemsElement = newPackageXml.Elements("DictionaryItems").First();

        // Act
        var xml = Serializer.Serialize(new[] { dictionaryItem });

        // Assert
        Assert.That(xml.ToString(), Is.EqualTo(dictionaryItemsElement.ToString()));
    }

    [Test]
    public async Task Can_Export_Languages()
    {
        // Arrange
        var languageService = GetRequiredService<ILanguageService>();

        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .WithCultureName("Norwegian Bokmål (Norway)")
            .Build();
        await languageService.CreateAsync(languageNbNo, Constants.Security.SuperUserKey);

        var languageEnGb = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .Build();
        await languageService.CreateAsync(languageEnGb, Constants.Security.SuperUserKey);

        var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
        var languageItemsElement = newPackageXml.Elements("Languages").First();

        // Act
        var xml = Serializer.Serialize(new[] { languageNbNo, languageEnGb });

        // Assert
        Assert.That(xml.ToString(), Is.EqualTo(languageItemsElement.ToString()));
    }

    [Test]
    public async Task Can_Generate_Xml_Representation_Of_Content()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey); // else, FK violation on contentType!
        var contentType = ContentTypeBuilder.CreateTextPageContentType(
            defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

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
        Assert.That((string)element.Attribute("id"), Is.EqualTo(content.Id.ToString()));
        Assert.That((string)element.Attribute("parentID"), Is.EqualTo(content.ParentId.ToString()));
        Assert.That((string)element.Attribute("level"), Is.EqualTo(content.Level.ToString()));
        Assert.That((string)element.Attribute("creatorID"), Is.EqualTo(content.CreatorId.ToString()));
        Assert.That((string)element.Attribute("sortOrder"), Is.EqualTo(content.SortOrder.ToString()));
        Assert.That((string)element.Attribute("createDate"), Is.EqualTo(content.CreateDate.ToString("s")));
        Assert.That((string)element.Attribute("updateDate"), Is.EqualTo(content.UpdateDate.ToString("s")));
        Assert.That((string)element.Attribute("nodeName"), Is.EqualTo(content.Name));
        Assert.That((string)element.Attribute("urlName"), Is.EqualTo(urlName));
        Assert.That((string)element.Attribute("path"), Is.EqualTo(content.Path));
        Assert.That((string)element.Attribute("isDoc"), Is.EqualTo(string.Empty));
        Assert.That((string)element.Attribute("nodeType"), Is.EqualTo(content.ContentType.Id.ToString()));
        Assert.That((string)element.Attribute("creatorName"), Is.EqualTo(content.GetCreatorProfile(UserService).Name));
        Assert.That((string)element.Attribute("writerName"), Is.EqualTo(content.GetWriterProfile(UserService).Name));
        Assert.That((string)element.Attribute("writerID"), Is.EqualTo(content.WriterId.ToString()));
        Assert.That((string)element.Attribute("template"), Is.EqualTo(content.TemplateId.ToString()));

        Assert.That(element.Elements("title").Single().Value, Is.EqualTo(content.Properties["title"].GetValue().ToString()));
        Assert.That(
            element.Elements("bodyText").Single().Value, Is.EqualTo(content.Properties["bodyText"].GetValue().ToString()));
        Assert.That(
            element.Elements("keywords").Single().Value, Is.EqualTo(content.Properties["keywords"].GetValue().ToString()));
        Assert.That(
            element.Elements("description").Single().Value, Is.EqualTo(content.Properties["description"].GetValue().ToString()));
    }

    [Test]
    public async Task Can_Generate_Xml_Representation_Of_Media()
    {
        // Arrange
        var mediaType = MediaTypeBuilder.CreateImageMediaType("image2");

        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);

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
            Services);

        var ignored = new FileUploadPropertyEditor(
            DataValueEditorFactory,
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
        Assert.That((string)element.Attribute("id"), Is.EqualTo(media.Id.ToString()));
        Assert.That((string)element.Attribute("parentID"), Is.EqualTo(media.ParentId.ToString()));
        Assert.That((string)element.Attribute("level"), Is.EqualTo(media.Level.ToString()));
        Assert.That((string)element.Attribute("sortOrder"), Is.EqualTo(media.SortOrder.ToString()));
        Assert.That((string)element.Attribute("createDate"), Is.EqualTo(media.CreateDate.ToString("s")));
        Assert.That((string)element.Attribute("updateDate"), Is.EqualTo(media.UpdateDate.ToString("s")));
        Assert.That((string)element.Attribute("nodeName"), Is.EqualTo(media.Name));
        Assert.That((string)element.Attribute("urlName"), Is.EqualTo(urlName));
        Assert.That((string)element.Attribute("path"), Is.EqualTo(media.Path));
        Assert.That((string)element.Attribute("isDoc"), Is.EqualTo(string.Empty));
        Assert.That((string)element.Attribute("nodeType"), Is.EqualTo(media.ContentType.Id.ToString()));
        Assert.That((string)element.Attribute("writerName"), Is.EqualTo(media.GetCreatorProfile(UserService).Name));
        Assert.That((string)element.Attribute("writerID"), Is.EqualTo(media.CreatorId.ToString()));
        Assert.That(element.Attribute("template"), Is.Null);

        Assert.That(element.Elements(Constants.Conventions.Media.File).Single().Value, Is.EqualTo(media.Properties[Constants.Conventions.Media.File].GetValue().ToString()));
        Assert.That(element.Elements(Constants.Conventions.Media.Width).Single().Value, Is.EqualTo(media.Properties[Constants.Conventions.Media.Width].GetValue().ToString()));
        Assert.That(element.Elements(Constants.Conventions.Media.Height).Single().Value, Is.EqualTo(media.Properties[Constants.Conventions.Media.Height].GetValue().ToString()));
        Assert.That(element.Elements(Constants.Conventions.Media.Bytes).Single().Value, Is.EqualTo(media.Properties[Constants.Conventions.Media.Bytes].GetValue().ToString()));
        Assert.That(element.Elements(Constants.Conventions.Media.Extension).Single().Value, Is.EqualTo(media.Properties[Constants.Conventions.Media.Extension].GetValue().ToString()));
    }

    [Test]
    public async Task Serialize_ForContentTypeWithHistoryCleanupPolicy_OutputsSerializedHistoryCleanupPolicy()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey); // else, FK violation on contentType!

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);

        contentType.HistoryCleanup = new HistoryCleanup
        {
            PreventCleanup = true,
            KeepAllVersionsNewerThanDays = 1,
            KeepLatestVersionPerDayForDays = 2
        };

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

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
    public async Task Serialize_ForContentTypeWithNullHistoryCleanupPolicy_DoesNotOutputSerializedDefaultPolicy()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey); // else, FK violation on contentType!

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);

        contentType.HistoryCleanup = null;

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var element = Serializer.Serialize(contentType);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(element.Element("HistoryCleanupPolicy"), Is.Null);
        });
    }

    private async Task CreateDictionaryData()
    {
        var languageService = GetRequiredService<ILanguageService>();
        var dictionaryItemService = GetRequiredService<IDictionaryItemService>();

        var languageNbNo = new LanguageBuilder()
            .WithCultureInfo("nb-NO")
            .WithCultureName("Norwegian")
            .Build();
        await languageService.CreateAsync(languageNbNo, Constants.Security.SuperUserKey);

        var languageEnGb = new LanguageBuilder()
            .WithCultureInfo("en-GB")
            .Build();
        await languageService.CreateAsync(languageEnGb, Constants.Security.SuperUserKey);

        var parentKey = Guid.Parse("28f2e02a-8c66-4fcd-85e3-8524d551c0d3");
        var parentItem = new DictionaryItem("Parent") { Key = parentKey };
        var parentTranslations = new List<IDictionaryTranslation>
        {
            new DictionaryTranslation(languageNbNo, "ForelderVerdi"),
            new DictionaryTranslation(languageEnGb, "ParentValue")
        };
        parentItem.Translations = parentTranslations;
        var result = await dictionaryItemService.CreateAsync(parentItem, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result.Key, Is.EqualTo(parentKey));

        var childKey = Guid.Parse("e7dba0a9-d517-4ba4-8e18-2764d392c611");
        var childItem =
            new DictionaryItem(parentItem.Key, "Child") { Key = childKey };
        var childTranslations = new List<IDictionaryTranslation>
        {
            new DictionaryTranslation(languageNbNo, "BarnVerdi"),
            new DictionaryTranslation(languageEnGb, "ChildValue")
        };
        childItem.Translations = childTranslations;
        result = await dictionaryItemService.CreateAsync(childItem, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result.Key, Is.EqualTo(childKey));
    }
}
