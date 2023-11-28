// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Formatters;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.BackOffice.Controllers;

[TestFixture]
public class ContentControllerTests : UmbracoTestServerTestBase
{
    private const string UsIso = "en-US";
    private const string DkIso = "da-DK";

    /// <summary>
    ///     Returns 404 if the content wasn't found based on the ID specified
    /// </summary>
    [Test]
    [LongRunning]
    public async Task PostSave_Validate_Existing_Content()
    {
        var languageService = GetRequiredService<ILanguageService>();

        // Add another language
        await languageService.CreateAsync(
            new LanguageBuilder()
            .WithCultureInfo(DkIso)
            .WithIsDefault(false)
            .Build(),
            Constants.Security.SuperUserKey);

        var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .AddPropertyType()
            .WithAlias("title")
            .WithValueStorageType(ValueStorageType.Integer)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithName("Title")
            .Done()
            .WithContentVariation(ContentVariation.Nothing)
            .Build();

        contentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName("Invariant")
            .WithContentType(contentType)
            .AddPropertyData()
            .WithKeyValue("title", "Cool invariant title")
            .Done()
            .Build();
        contentService.Save(content);
        contentService.Publish(content, Array.Empty<string>());

        var model = new ContentItemSaveBuilder()
            .WithContent(content)
            .WithId(-1337) // HERE We overwrite the Id, so we don't expect to find it on the server
            .Build();

        // Act
        var response =
            await Client.PostAsync(url, new MultipartFormDataContent {{new StringContent(JsonConvert.SerializeObject(model)), "contentItem"}});

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        //// Assert.AreEqual(")]}',\n{\"Message\":\"content was not found\"}", response.Item1.Content.ReadAsStringAsync().Result);
        ////
        ////             //var obj = JsonConvert.DeserializeObject<PagedResult<UserDisplay>>(response.Item2);
        ////             //Assert.AreEqual(0, obj.TotalItems);
    }

    [Test]
    [LongRunning]
    public async Task PostSave_Validate_At_Least_One_Variant_Flagged_For_Saving()
    {
        var languageService = GetRequiredService<ILanguageService>();

        // Add another language
        await languageService.CreateAsync(
            new LanguageBuilder()
            .WithCultureInfo(DkIso)
            .WithIsDefault(false)
            .Build(),
            Constants.Security.SuperUserKey);

        var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .AddPropertyType()
            .WithAlias("title")
            .WithValueStorageType(ValueStorageType.Integer)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithName("Title")
            .Done()
            .WithContentVariation(ContentVariation.Nothing)
            .Build();

        contentTypeService.Save(contentType);

        var contentService = GetRequiredService<IContentService>();
        var content = new ContentBuilder()
            .WithId(0)
            .WithName("Invariant")
            .WithContentType(contentType)
            .AddPropertyData()
            .WithKeyValue("title", "Cool invariant title")
            .Done()
            .Build();
        contentService.Save(content);
        contentService.Publish(content, Array.Empty<string>());

        var model = new ContentItemSaveBuilder()
            .WithContent(content)
            .Build();

        // HERE we force the test to fail
        model.Variants = model.Variants.Select(x =>
        {
            x.Save = false;
            return x;
        });

        // Act
        var response =
            await Client.PostAsync(url, new MultipartFormDataContent {{new StringContent(JsonConvert.SerializeObject(model)), "contentItem"}});

        // Assert
        var body = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual(
                AngularJsonMediaTypeFormatter.XsrfPrefix + "{\"Message\":\"No variants flagged for saving\"}", body);
        });
    }

    /// <summary>
    ///     Returns 404 if any of the posted properties dont actually exist
    /// </summary>
    [Test]
    [LongRunning]
    public async Task PostSave_Validate_Properties_Exist()
    {
        var languageService = GetRequiredService<ILanguageService>();

        // Add another language
        await languageService.CreateAsync(
            new LanguageBuilder()
            .WithCultureInfo(DkIso)
            .WithIsDefault(false)
            .Build(),
            Constants.Security.SuperUserKey);

        var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .AddPropertyType()
            .WithAlias("title")
            .WithValueStorageType(ValueStorageType.Integer)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithName("Title")
            .Done()
            .WithContentVariation(ContentVariation.Nothing)
            .Build();

        contentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName("Invariant")
            .WithContentType(contentType)
            .AddPropertyData()
            .WithKeyValue("title", "Cool invariant title")
            .Done()
            .Build();
        contentService.Save(content);
        contentService.Publish(content, Array.Empty<string>());

        var model = new ContentItemSaveBuilder()
            .WithId(content.Id)
            .WithContentTypeAlias(content.ContentType.Alias)
            .AddVariant()
            .AddProperty()
            .WithId(2)
            .WithAlias("doesntexists")
            .WithValue("Whatever")
            .Done()
            .Done()
            .Build();

        // Act
        var response =
            await Client.PostAsync(url, new MultipartFormDataContent {{new StringContent(JsonConvert.SerializeObject(model)), "contentItem"}});

        // Assert
        var body = await response.Content.ReadAsStringAsync();

        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    [LongRunning]
    public async Task PostSave_Simple_Invariant()
    {
        var languageService = GetRequiredService<ILanguageService>();

        // Add another language
        await languageService.CreateAsync(
            new LanguageBuilder()
            .WithCultureInfo(DkIso)
            .WithIsDefault(false)
            .Build(),
            Constants.Security.SuperUserKey);

        var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .AddPropertyType()
            .WithAlias("title")
            .WithValueStorageType(ValueStorageType.Integer)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithName("Title")
            .Done()
            .WithContentVariation(ContentVariation.Nothing)
            .Build();

        contentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName("Invariant")
            .WithContentType(contentType)
            .AddPropertyData()
            .WithKeyValue("title", "Cool invariant title")
            .Done()
            .Build();
        contentService.Save(content);
        contentService.Publish(content, Array.Empty<string>());
        var model = new ContentItemSaveBuilder()
            .WithContent(content)
            .Build();

        // Act
        var response =
            await Client.PostAsync(url, new MultipartFormDataContent {{new StringContent(JsonConvert.SerializeObject(model)), "contentItem"}});

        // Assert
        var body = await response.Content.ReadAsStringAsync();

        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, body);
            var display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);
            Assert.AreEqual(1, display.Variants.Count());
        });
    }

    [Test]
    [LongRunning]
    public async Task PostSave_Validate_Empty_Name()
    {
        var languageService = GetRequiredService<ILanguageService>();

        // Add another language
        await languageService.CreateAsync(
            new LanguageBuilder()
            .WithCultureInfo(DkIso)
            .WithIsDefault(false)
            .Build(),
            Constants.Security.SuperUserKey);

        var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .AddPropertyType()
            .WithAlias("title")
            .WithValueStorageType(ValueStorageType.Integer)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithName("Title")
            .Done()
            .WithContentVariation(ContentVariation.Nothing)
            .Build();

        contentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(0)
            .WithName("Invariant")
            .WithContentType(contentType)
            .AddPropertyData()
            .WithKeyValue("title", "Cool invariant title")
            .Done()
            .Build();
        contentService.Save(content);
        contentService.Publish(content, Array.Empty<string>());

        content.Name = null; // Removes the name of one of the variants to force an error
        var model = new ContentItemSaveBuilder()
            .WithContent(content)
            .Build();

        // Act
        var response =
            await Client.PostAsync(url, new MultipartFormDataContent {{new StringContent(JsonConvert.SerializeObject(model)), "contentItem"}});

        // Assert
        var body = await response.Content.ReadAsStringAsync();

        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);
            Assert.AreEqual(1, display.Errors.Count(), string.Join(",", display.Errors));
            CollectionAssert.Contains(display.Errors.Keys, "Variants[0].Name");
        });
    }

    [Test]
    [LongRunning]
    public async Task PostSave_Validate_Variants_Empty_Name()
    {
        var languageService = GetRequiredService<ILanguageService>();

        // Add another language
        await languageService.CreateAsync(
            new LanguageBuilder()
            .WithCultureInfo(DkIso)
            .WithIsDefault(false)
            .Build(),
            Constants.Security.SuperUserKey);

        var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

        var contentService = GetRequiredService<IContentService>();
        var contentTypeService = GetRequiredService<IContentTypeService>();

        var contentType = new ContentTypeBuilder()
            .WithId(0)
            .AddPropertyType()
            .WithAlias("title")
            .WithValueStorageType(ValueStorageType.Integer)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .WithName("Title")
            .Done()
            .WithContentVariation(ContentVariation.Culture)
            .Build();

        contentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(0)
            .WithCultureName(UsIso, "English")
            .WithCultureName(DkIso, "Danish")
            .WithContentType(contentType)
            .AddPropertyData()
            .WithKeyValue("title", "Cool invariant title")
            .Done()
            .Build();
        contentService.Save(content);
        contentService.Publish(content, content.AvailableCultures.ToArray());

        content.CultureInfos[0].Name = null; // Removes the name of one of the variants to force an error
        var model = new ContentItemSaveBuilder()
            .WithContent(content)
            .Build();

        // Act
        var response =
            await Client.PostAsync(url, new MultipartFormDataContent {{new StringContent(JsonConvert.SerializeObject(model)), "contentItem"}});

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);
            Assert.AreEqual(2, display.Errors.Count());
            CollectionAssert.Contains(display.Errors.Keys, "Variants[0].Name");
            CollectionAssert.Contains(display.Errors.Keys, "_content_variant_en-US_null_");
        });
    }

    [Test]
    [LongRunning]
    public async Task PostSave_Validates_Domains_Exist()
    {
        var languageService = GetRequiredService<ILanguageService>();
        await languageService.CreateAsync(
            new LanguageBuilder()
            .WithCultureInfo(DkIso)
            .WithIsDefault(false)
            .Build(),
            Constants.Security.SuperUserKey);

        var contentTypeService = GetRequiredService<IContentTypeService>();
        var contentType = new ContentTypeBuilder().WithContentVariation(ContentVariation.Culture).Build();
        contentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithId(1)
            .WithContentType(contentType)
            .WithCultureName(UsIso, "Root")
            .WithCultureName(DkIso, "Rod")
            .Build();

        var model = new ContentItemSaveBuilder()
            .WithContent(content)
            .WithAction(ContentSaveAction.PublishNew)
            .Build();

        var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

        var response =
            await Client.PostAsync(url, new MultipartFormDataContent {{new StringContent(JsonConvert.SerializeObject(model)), "contentItem"}});

        var body = await response.Content.ReadAsStringAsync();
        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
        var display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);

        var localizedTextService = GetRequiredService<ILocalizedTextService>();
        var expectedMessage = localizedTextService.Localize("speechBubbles", "publishWithNoDomains");

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(display);
            Assert.AreEqual(1, display.Notifications.Count(x => x.NotificationType == NotificationStyle.Warning));
            Assert.AreEqual(
                expectedMessage,
                display.Notifications.FirstOrDefault(x => x.NotificationType == NotificationStyle.Warning)?.Message);
        });
    }

    [Test]
    [LongRunning]
    public async Task PostSave_Validates_All_Ancestor_Cultures_Are_Considered()
    {
        var sweIso = "sv-SE";
        var languageService = GetRequiredService<ILanguageService>();
        //Create 2 new languages
        await languageService.CreateAsync(
            new LanguageBuilder()
            .WithCultureInfo(DkIso)
            .WithIsDefault(false)
            .Build(),
            Constants.Security.SuperUserKey);

        await languageService.CreateAsync(
            new LanguageBuilder()
            .WithCultureInfo(sweIso)
            .WithIsDefault(false)
            .Build(),
            Constants.Security.SuperUserKey);

        var contentTypeService = GetRequiredService<IContentTypeService>();
        var contentType = new ContentTypeBuilder().WithContentVariation(ContentVariation.Culture).Build();
        contentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithoutIdentity()
            .WithContentType(contentType)
            .WithCultureName(UsIso, "Root")
            .Build();

        var contentService = GetRequiredService<IContentService>();
        contentService.Save(content);
        contentService.Publish(content, content.AvailableCultures.ToArray());

        var childContent = new ContentBuilder()
            .WithoutIdentity()
            .WithContentType(contentType)
            .WithParent(content)
            .WithCultureName(DkIso, "Barn")
            .WithCultureName(UsIso, "Child")
            .Build();

        contentService.Save(childContent);
        contentService.Publish(childContent, content.AvailableCultures.ToArray());

        var grandChildContent = new ContentBuilder()
            .WithoutIdentity()
            .WithContentType(contentType)
            .WithParent(childContent)
            .WithCultureName(sweIso, "Bjarn")
            .Build();


        var model = new ContentItemSaveBuilder()
            .WithContent(grandChildContent)
            .WithParentId(childContent.Id)
            .WithAction(ContentSaveAction.PublishNew)
            .Build();

        var enLanguage = await languageService.GetAsync(UsIso);
        var dkLanguage = await languageService.GetAsync(DkIso);

        var domainService = GetRequiredService<IDomainService>();

        await domainService.UpdateDomainsAsync(
            content.Key,
            new DomainsUpdateModel
            {
                Domains = new[] { new DomainModel { DomainName = "/en", IsoCode = enLanguage.IsoCode } }
            });

        await domainService.UpdateDomainsAsync(
            childContent.Key,
            new DomainsUpdateModel
            {
                Domains = new[] { new DomainModel { DomainName = "/dk", IsoCode = dkLanguage.IsoCode } }
            });

        var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

        var result = JsonConvert.SerializeObject(model);
        var response =
            await Client.PostAsync(url, new MultipartFormDataContent {{new StringContent(JsonConvert.SerializeObject(model)), "contentItem"}});

        var body = await response.Content.ReadAsStringAsync();
        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
        var display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);


        var localizedTextService = GetRequiredService<ILocalizedTextService>();
        var expectedMessage =
            localizedTextService.Localize("speechBubbles", "publishWithMissingDomain", new[] {"sv-SE"});

        Assert.Multiple(() =>
        {
            Assert.NotNull(display);
            Assert.AreEqual(1, display.Notifications.Count(x => x.NotificationType == NotificationStyle.Warning));
            Assert.AreEqual(
                expectedMessage,
                display.Notifications.FirstOrDefault(x => x.NotificationType == NotificationStyle.Warning)?.Message);
        });
    }

    [Test]
    [LongRunning]
    public async Task PostSave_Validates_All_Cultures_Has_Domains()
    {
        var languageService = GetRequiredService<ILanguageService>();
        await languageService.CreateAsync(
            new LanguageBuilder()
            .WithCultureInfo(DkIso)
            .WithIsDefault(false)
            .Build(),
            Constants.Security.SuperUserKey);

        var contentTypeService = GetRequiredService<IContentTypeService>();
        var contentType = new ContentTypeBuilder().WithContentVariation(ContentVariation.Culture).Build();
        contentTypeService.Save(contentType);

        var content = new ContentBuilder()
            .WithoutIdentity()
            .WithContentType(contentType)
            .WithCultureName(UsIso, "Root")
            .WithCultureName(DkIso, "Rod")
            .Build();

        var contentService = GetRequiredService<IContentService>();
        contentService.Save(content);

        var model = new ContentItemSaveBuilder()
            .WithContent(content)
            .WithAction(ContentSaveAction.Publish)
            .Build();

        var dkLanguage = await languageService.GetAsync(DkIso);
        var domainService = GetRequiredService<IDomainService>();

        await domainService.UpdateDomainsAsync(
            content.Key,
            new DomainsUpdateModel
            {
                Domains = new[] { new DomainModel { DomainName = "/", IsoCode = dkLanguage.IsoCode } }
            });

        var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

        var response =
            await Client.PostAsync(url, new MultipartFormDataContent {{new StringContent(JsonConvert.SerializeObject(model)), "contentItem"}});

        var body = await response.Content.ReadAsStringAsync();
        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
        var display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);


        var localizedTextService = GetRequiredService<ILocalizedTextService>();
        var expectedMessage = localizedTextService.Localize("speechBubbles", "publishWithMissingDomain", new[] {UsIso});

        Assert.Multiple(() =>
        {
            Assert.NotNull(display);
            Assert.AreEqual(1, display.Notifications.Count(x => x.NotificationType == NotificationStyle.Warning));
            Assert.AreEqual(
                expectedMessage,
                display.Notifications.FirstOrDefault(x => x.NotificationType == NotificationStyle.Warning)?.Message);
        });
    }

    [Test]
    [LongRunning]
    public async Task PostSave_Checks_Ancestors_For_Domains()
    {
        var languageService = GetRequiredService<ILanguageService>();
        await languageService.CreateAsync(
            new LanguageBuilder()
            .WithCultureInfo(DkIso)
            .WithIsDefault(false)
            .Build(),
            Constants.Security.SuperUserKey);

        var contentTypeService = GetRequiredService<IContentTypeService>();
        var contentType = new ContentTypeBuilder().WithContentVariation(ContentVariation.Culture).Build();
        contentTypeService.Save(contentType);

        var rootNode = new ContentBuilder()
            .WithoutIdentity()
            .WithContentType(contentType)
            .WithCultureName(UsIso, "Root")
            .WithCultureName(DkIso, "Rod")
            .Build();

        var contentService = GetRequiredService<IContentService>();
        contentService.Save(rootNode);
        contentService.Publish(rootNode, rootNode.AvailableCultures.ToArray());

        var childNode = new ContentBuilder()
            .WithoutIdentity()
            .WithParent(rootNode)
            .WithContentType(contentType)
            .WithCultureName(DkIso, "Barn")
            .WithCultureName(UsIso, "Child")
            .Build();

        contentService.Save(childNode);
        contentService.Publish(childNode, childNode.AvailableCultures.ToArray());

        var grandChild = new ContentBuilder()
            .WithoutIdentity()
            .WithParent(childNode)
            .WithContentType(contentType)
            .WithCultureName(DkIso, "BarneBarn")
            .WithCultureName(UsIso, "GrandChild")
            .Build();

        contentService.Save(grandChild);

        var dkLanguage = await languageService.GetAsync(DkIso);
        var usLanguage = await languageService.GetAsync(UsIso);
        var domainService = GetRequiredService<IDomainService>();

        await domainService.UpdateDomainsAsync(
            rootNode.Key,
            new DomainsUpdateModel
            {
                Domains = new[] { new DomainModel { DomainName = "/", IsoCode = dkLanguage.IsoCode } }
            });

        await domainService.UpdateDomainsAsync(
            childNode.Key,
            new DomainsUpdateModel
            {
                Domains = new[] { new DomainModel { DomainName = "/en", IsoCode = usLanguage.IsoCode } }
            });

        var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

        var model = new ContentItemSaveBuilder()
            .WithContent(grandChild)
            .WithAction(ContentSaveAction.Publish)
            .Build();

        var response =
            await Client.PostAsync(url, new MultipartFormDataContent {{new StringContent(JsonConvert.SerializeObject(model)), "contentItem"}});

        var body = await response.Content.ReadAsStringAsync();
        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
        var display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);

        Assert.Multiple(() =>
        {
            Assert.NotNull(display);
            // Assert all is good, a success notification for each culture published and no warnings.
            Assert.AreEqual(2, display.Notifications.Count(x => x.NotificationType == NotificationStyle.Success));
            Assert.AreEqual(0, display.Notifications.Count(x => x.NotificationType == NotificationStyle.Warning));
        });
    }

    // FIXME: I've commented out this tests, since it should be relevant for new backoffice
    // This is because in new backoffice we use the temporary file service to handle drag'n'dropped images
    // see RichTextEditorPastedImages.FindAndPersistPastedTempImagesAsync for more information.
    // [TestCase(
    //     @"<p><img alt src=""data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7""></p>",
    //     false)]
    // [TestCase(
    //     @"<p><img src=""data:image/svg+xml;utf8,<svg viewBox=""0 0 70 74"" fill=""none"" xmlns=""http://www.w3.org/2000/svg""><rect width=""100%"" height=""100%"" fill=""black""/></svg>""></p>",
    //     false)]
    // [TestCase(
    //     @"<p><img alt src=""/some/random/image.jpg""></p><p><img alt src=""data:image/jpg;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7""></p>",
    //     false)]
    // [TestCase(
    //     @"<p><img alt src=""data:image/notallowedextension;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7""></p>",
    //     true)]
    // [LongRunning]
    // public async Task PostSave_Simple_RichText_With_Base64(string html, bool shouldHaveDataUri)
    // {
    //     var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));
    //
    //     var dataTypeService = GetRequiredService<IDataTypeService>();
    //     var contentService = GetRequiredService<IContentService>();
    //     var contentTypeService = GetRequiredService<IContentTypeService>();
    //
    //     var dataType = new DataTypeBuilder()
    //         .WithId(0)
    //         .WithoutIdentity()
    //         .WithDatabaseType(ValueStorageType.Ntext)
    //         .AddEditor()
    //         .WithAlias(Constants.PropertyEditors.Aliases.TinyMce)
    //         .Done()
    //         .Build();
    //
    //     dataTypeService.Save(dataType);
    //
    //     var contentType = new ContentTypeBuilder()
    //         .WithId(0)
    //         .AddPropertyType()
    //         .WithDataTypeId(dataType.Id)
    //         .WithAlias("richText")
    //         .WithName("Rich Text")
    //         .Done()
    //         .WithContentVariation(ContentVariation.Nothing)
    //         .Build();
    //
    //     contentTypeService.Save(contentType);
    //
    //     var content = new ContentBuilder()
    //         .WithId(0)
    //         .WithName("Invariant")
    //         .WithContentType(contentType)
    //         .AddPropertyData()
    //         .WithKeyValue("richText", html)
    //         .Done()
    //         .Build();
    //     contentService.SaveAndPublish(content);
    //     var model = new ContentItemSaveBuilder()
    //         .WithContent(content)
    //         .Build();
    //
    //     // Act
    //     var response =
    //         await Client.PostAsync(url, new MultipartFormDataContent {{new StringContent(JsonConvert.SerializeObject(model)), "contentItem"}});
    //
    //     // Assert
    //     var body = await response.Content.ReadAsStringAsync();
    //
    //     body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
    //
    //     Assert.Multiple(() =>
    //     {
    //         Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, body);
    //         var display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);
    //         var bodyText = display.Variants.FirstOrDefault()?.Tabs.FirstOrDefault()?.Properties
    //             ?.FirstOrDefault(x => x.Alias.Equals("richText"))?.Value?.ToString();
    //         Assert.NotNull(bodyText);
    //
    //         var containsDataUri = bodyText.Contains("data:image");
    //         if (shouldHaveDataUri)
    //         {
    //             Assert.True(containsDataUri, $"Data URIs were expected to be found in the body: {bodyText}");
    //         } else {
    //             Assert.False(containsDataUri, $"Data URIs were not expected to be found in the body: {bodyText}");
    //         }
    //     });
    // }
}
