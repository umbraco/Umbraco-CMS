// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Formatters;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.BackOffice.Controllers
{
    [TestFixture]
    public class ContentControllerTests : UmbracoTestServerTestBase
    {
        private const string UsIso = "en-US";
        private const string DkIso = "da-DK";

        /// <summary>
        ///     Returns 404 if the content wasn't found based on the ID specified
        /// </summary>
        [Test]
        public async Task PostSave_Validate_Existing_Content()
        {
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();

            // Add another language
            localizationService.Save(new LanguageBuilder()
                .WithCultureInfo(DkIso)
                .WithIsDefault(false)
                .Build());

            var pathBase = string.Empty;
            string url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null), pathBase);

            IContentService contentService = GetRequiredService<IContentService>();
            IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();

            IContentType contentType = new ContentTypeBuilder()
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

            Content content = new ContentBuilder()
                .WithId(0)
                .WithName("Invariant")
                .WithContentType(contentType)
                .AddPropertyData()
                .WithKeyValue("title", "Cool invariant title")
                .Done()
                .Build();
            contentService.SaveAndPublish(content);

            ContentItemSave model = new ContentItemSaveBuilder()
                .WithContent(content)
                .WithId(-1337) // HERE We overwrite the Id, so we don't expect to find it on the server
                .Build();

            // Act
            HttpResponseMessage response = await Client.PostAsync(url, new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(model)), "contentItem" }
            });

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            //// Assert.AreEqual(")]}',\n{\"Message\":\"content was not found\"}", response.Item1.Content.ReadAsStringAsync().Result);
            ////
            ////             //var obj = JsonConvert.DeserializeObject<PagedResult<UserDisplay>>(response.Item2);
            ////             //Assert.AreEqual(0, obj.TotalItems);
        }

        [Test]
        public async Task PostSave_Validate_At_Least_One_Variant_Flagged_For_Saving()
        {
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();

            // Add another language
            localizationService.Save(new LanguageBuilder()
                .WithCultureInfo(DkIso)
                .WithIsDefault(false)
                .Build());

            var pathBase = string.Empty;
            string url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null), pathBase);

            IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();

            IContentType contentType = new ContentTypeBuilder()
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

            IContentService contentService = GetRequiredService<IContentService>();
            Content content = new ContentBuilder()
                .WithId(0)
                .WithName("Invariant")
                .WithContentType(contentType)
                .AddPropertyData()
                .WithKeyValue("title", "Cool invariant title")
                .Done()
                .Build();
            contentService.SaveAndPublish(content);

            ContentItemSave model = new ContentItemSaveBuilder()
                .WithContent(content)
                .Build();

            // HERE we force the test to fail
            model.Variants = model.Variants.Select(x =>
            {
                x.Save = false;
                return x;
            });

            // Act
            HttpResponseMessage response = await Client.PostAsync(url, new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(model)), "contentItem" }
            });

            // Assert
            string body = await response.Content.ReadAsStringAsync();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                Assert.AreEqual(AngularJsonMediaTypeFormatter.XsrfPrefix + "{\"Message\":\"No variants flagged for saving\"}", body);
            });
        }

        /// <summary>
        ///     Returns 404 if any of the posted properties dont actually exist
        /// </summary>
        [Test]
        public async Task PostSave_Validate_Properties_Exist()
        {
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();

            // Add another language
            localizationService.Save(new LanguageBuilder()
                .WithCultureInfo(DkIso)
                .WithIsDefault(false)
                .Build());

            var pathBase = string.Empty;
            string url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null), pathBase);

            IContentService contentService = GetRequiredService<IContentService>();
            IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();

            IContentType contentType = new ContentTypeBuilder()
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

            Content content = new ContentBuilder()
                .WithId(0)
                .WithName("Invariant")
                .WithContentType(contentType)
                .AddPropertyData()
                .WithKeyValue("title", "Cool invariant title")
                .Done()
                .Build();
            contentService.SaveAndPublish(content);

            ContentItemSave model = new ContentItemSaveBuilder()
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
            HttpResponseMessage response = await Client.PostAsync(url, new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(model)), "contentItem" }
            });

            // Assert
            string body = await response.Content.ReadAsStringAsync();

            body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task PostSave_Simple_Invariant()
        {
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();

            // Add another language
            localizationService.Save(new LanguageBuilder()
                .WithCultureInfo(DkIso)
                .WithIsDefault(false)
                .Build());

            var pathBase = string.Empty;
            string url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null), pathBase);

            IContentService contentService = GetRequiredService<IContentService>();
            IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();

            IContentType contentType = new ContentTypeBuilder()
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

            Content content = new ContentBuilder()
                .WithId(0)
                .WithName("Invariant")
                .WithContentType(contentType)
                .AddPropertyData()
                .WithKeyValue("title", "Cool invariant title")
                .Done()
                .Build();
            contentService.SaveAndPublish(content);
            ContentItemSave model = new ContentItemSaveBuilder()
                .WithContent(content)
                .Build();

            // Act
            HttpResponseMessage response = await Client.PostAsync(url, new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(model)), "contentItem" }
            });

            // Assert
            string body = await response.Content.ReadAsStringAsync();

            body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, body);
                ContentItemDisplay display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);
                Assert.AreEqual(1, display.Variants.Count());
            });
        }

        [Test]
        public async Task PostSave_Validate_Empty_Name()
        {
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();

            // Add another language
            localizationService.Save(new LanguageBuilder()
                .WithCultureInfo(DkIso)
                .WithIsDefault(false)
                .Build());

            var pathBase = string.Empty;
            string url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null), pathBase);

            IContentService contentService = GetRequiredService<IContentService>();
            IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();

            IContentType contentType = new ContentTypeBuilder()
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

            Content content = new ContentBuilder()
                .WithId(0)
                .WithName("Invariant")
                .WithContentType(contentType)
                .AddPropertyData()
                .WithKeyValue("title", "Cool invariant title")
                .Done()
                .Build();
            contentService.SaveAndPublish(content);

            content.Name = null; // Removes the name of one of the variants to force an error
            ContentItemSave model = new ContentItemSaveBuilder()
                .WithContent(content)
                .Build();

            // Act
            HttpResponseMessage response = await Client.PostAsync(url, new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(model)), "contentItem" }
            });

            // Assert
            string body = await response.Content.ReadAsStringAsync();

            body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
                ContentItemDisplay display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);
                Assert.AreEqual(1, display.Errors.Count(), string.Join(",", display.Errors));
                CollectionAssert.Contains(display.Errors.Keys, "Variants[0].Name");
            });
        }

        [Test]
        public async Task PostSave_Validate_Variants_Empty_Name()
        {
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();

            // Add another language
            localizationService.Save(new LanguageBuilder()
                .WithCultureInfo(DkIso)
                .WithIsDefault(false)
                .Build());

            var pathBase = string.Empty;
            string url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null), pathBase);

            IContentService contentService = GetRequiredService<IContentService>();
            IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();

            IContentType contentType = new ContentTypeBuilder()
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

            Content content = new ContentBuilder()
                .WithId(0)
                .WithCultureName(UsIso, "English")
                .WithCultureName(DkIso, "Danish")
                .WithContentType(contentType)
                .AddPropertyData()
                .WithKeyValue("title", "Cool invariant title")
                .Done()
                .Build();
            contentService.SaveAndPublish(content);

            content.CultureInfos[0].Name = null; // Removes the name of one of the variants to force an error
            ContentItemSave model = new ContentItemSaveBuilder()
                .WithContent(content)
                .Build();

            // Act
            HttpResponseMessage response = await Client.PostAsync(url, new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(model)), "contentItem" }
            });

            // Assert
            string body = await response.Content.ReadAsStringAsync();
            body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
                ContentItemDisplay display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);
                Assert.AreEqual(2, display.Errors.Count());
                CollectionAssert.Contains(display.Errors.Keys, "Variants[0].Name");
                CollectionAssert.Contains(display.Errors.Keys, "_content_variant_en-US_null_");
            });
        }

        [Test]
        public async Task PostSave_Validates_Domains_Exist()
        {
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();
            localizationService.Save(new LanguageBuilder()
                .WithCultureInfo(DkIso)
                .WithIsDefault(false)
                .Build());

            IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();
            IContentType contentType = new ContentTypeBuilder().WithContentVariation(ContentVariation.Culture).Build();
            contentTypeService.Save(contentType);

            Content content = new ContentBuilder()
                .WithId(1)
                .WithContentType(contentType)
                .WithCultureName(UsIso, "Root")
                .WithCultureName(DkIso, "Rod")
                .Build();

            ContentItemSave model = new ContentItemSaveBuilder()
                .WithContent(content)
                .WithAction(ContentSaveAction.PublishNew)
                .Build();

            var pathBase = string.Empty;
            var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null), pathBase);

            HttpResponseMessage response = await Client.PostAsync(url, new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(model)), "contentItem" }
            });

            var body = await response.Content.ReadAsStringAsync();
            body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
            ContentItemDisplay display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);

            ILocalizedTextService localizedTextService = GetRequiredService<ILocalizedTextService>();
            var expectedMessage = localizedTextService.Localize("speechBubbles", "publishWithNoDomains");

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(display);
                Assert.AreEqual(1, display.Notifications.Count(x => x.NotificationType == NotificationStyle.Warning));
                Assert.AreEqual(expectedMessage, display.Notifications.FirstOrDefault(x => x.NotificationType == NotificationStyle.Warning)?.Message);
            });
        }

        [Test]
        public async Task PostSave_Validates_All_Ancestor_Cultures_Are_Considered()
        {
            var sweIso = "sv-SE";
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();
            //Create 2 new languages
            localizationService.Save(new LanguageBuilder()
                .WithCultureInfo(DkIso)
                .WithIsDefault(false)
                .Build());

            localizationService.Save(new LanguageBuilder()
                .WithCultureInfo(sweIso)
                .WithIsDefault(false)
                .Build());

            IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();
            IContentType contentType = new ContentTypeBuilder().WithContentVariation(ContentVariation.Culture).Build();
            contentTypeService.Save(contentType);

            Content content = new ContentBuilder()
                .WithoutIdentity()
                .WithContentType(contentType)
                .WithCultureName(UsIso, "Root")
                .Build();

            IContentService contentService = GetRequiredService<IContentService>();
            contentService.SaveAndPublish(content);

            Content childContent = new ContentBuilder()
                .WithoutIdentity()
                .WithContentType(contentType)
                .WithParent(content)
                .WithCultureName(DkIso, "Barn")
                .WithCultureName(UsIso, "Child")
                .Build();

            contentService.SaveAndPublish(childContent);

            Content grandChildContent = new ContentBuilder()
                .WithoutIdentity()
                .WithContentType(contentType)
                .WithParent(childContent)
                .WithCultureName(sweIso, "Bjarn")
                .Build();


            ContentItemSave model = new ContentItemSaveBuilder()
                .WithContent(grandChildContent)
                .WithParentId(childContent.Id)
                .WithAction(ContentSaveAction.PublishNew)
                .Build();

            ILanguage enLanguage = localizationService.GetLanguageByIsoCode(UsIso);
            IDomainService domainService = GetRequiredService<IDomainService>();
            var enDomain = new UmbracoDomain("/en")
            {
                RootContentId = content.Id,
                LanguageId = enLanguage.Id
            };
            domainService.Save(enDomain);

            ILanguage dkLanguage = localizationService.GetLanguageByIsoCode(DkIso);
            var dkDomain = new UmbracoDomain("/dk")
            {
                RootContentId = childContent.Id,
                LanguageId = dkLanguage.Id
            };
            domainService.Save(dkDomain);

            var pathBase = string.Empty;
            var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null), pathBase);

            HttpResponseMessage response = await Client.PostAsync(url, new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(model)), "contentItem" }
            });

            var body = await response.Content.ReadAsStringAsync();
            body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
            ContentItemDisplay display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);


            ILocalizedTextService localizedTextService = GetRequiredService<ILocalizedTextService>();
            var expectedMessage = localizedTextService.Localize("speechBubbles", "publishWithMissingDomain", new []{"sv-SE"});

            Assert.Multiple(() =>
            {
                Assert.NotNull(display);
                Assert.AreEqual(1, display.Notifications.Count(x => x.NotificationType == NotificationStyle.Warning));
                Assert.AreEqual(expectedMessage, display.Notifications.FirstOrDefault(x => x.NotificationType == NotificationStyle.Warning)?.Message);
            });
        }

        [Test]
        public async Task PostSave_Validates_All_Cultures_Has_Domains()
        {
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();
            localizationService.Save(new LanguageBuilder()
                .WithCultureInfo(DkIso)
                .WithIsDefault(false)
                .Build());

            IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();
            IContentType contentType = new ContentTypeBuilder().WithContentVariation(ContentVariation.Culture).Build();
            contentTypeService.Save(contentType);

            Content content = new ContentBuilder()
                .WithoutIdentity()
                .WithContentType(contentType)
                .WithCultureName(UsIso, "Root")
                .WithCultureName(DkIso, "Rod")
                .Build();

            IContentService contentService = GetRequiredService<IContentService>();
            contentService.Save(content);

            ContentItemSave model = new ContentItemSaveBuilder()
                .WithContent(content)
                .WithAction(ContentSaveAction.Publish)
                .Build();

            ILanguage dkLanguage = localizationService.GetLanguageByIsoCode(DkIso);
            IDomainService domainService = GetRequiredService<IDomainService>();
            var dkDomain = new UmbracoDomain("/")
            {
                RootContentId = content.Id,
                LanguageId = dkLanguage.Id
            };
            domainService.Save(dkDomain);

            var pathBase = string.Empty;
            var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null), pathBase);

            HttpResponseMessage response = await Client.PostAsync(url, new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(model)), "contentItem" }
            });

            var body = await response.Content.ReadAsStringAsync();
            body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
            ContentItemDisplay display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);


            ILocalizedTextService localizedTextService = GetRequiredService<ILocalizedTextService>();
            var expectedMessage = localizedTextService.Localize("speechBubbles", "publishWithMissingDomain", new []{UsIso});

            Assert.Multiple(() =>
            {
                Assert.NotNull(display);
                Assert.AreEqual(1, display.Notifications.Count(x => x.NotificationType == NotificationStyle.Warning));
                Assert.AreEqual(expectedMessage, display.Notifications.FirstOrDefault(x => x.NotificationType == NotificationStyle.Warning)?.Message);
            });
        }

        [Test]
        public async Task PostSave_Checks_Ancestors_For_Domains()
        {
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();
            localizationService.Save(new LanguageBuilder()
                .WithCultureInfo(DkIso)
                .WithIsDefault(false)
                .Build());

            IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();
            IContentType contentType = new ContentTypeBuilder().WithContentVariation(ContentVariation.Culture).Build();
            contentTypeService.Save(contentType);

            Content rootNode = new ContentBuilder()
                .WithoutIdentity()
                .WithContentType(contentType)
                .WithCultureName(UsIso, "Root")
                .WithCultureName(DkIso, "Rod")
                .Build();

            IContentService contentService = GetRequiredService<IContentService>();
            contentService.SaveAndPublish(rootNode);

            Content childNode = new ContentBuilder()
                .WithoutIdentity()
                .WithParent(rootNode)
                .WithContentType(contentType)
                .WithCultureName(DkIso, "Barn")
                .WithCultureName(UsIso, "Child")
                .Build();

            contentService.SaveAndPublish(childNode);

            Content grandChild = new ContentBuilder()
                .WithoutIdentity()
                .WithParent(childNode)
                .WithContentType(contentType)
                .WithCultureName(DkIso, "BarneBarn")
                .WithCultureName(UsIso, "GrandChild")
                .Build();

            contentService.Save(grandChild);

            ILanguage dkLanguage = localizationService.GetLanguageByIsoCode(DkIso);
            ILanguage usLanguage = localizationService.GetLanguageByIsoCode(UsIso);
            IDomainService domainService = GetRequiredService<IDomainService>();
            var dkDomain = new UmbracoDomain("/")
            {
                RootContentId = rootNode.Id,
                LanguageId = dkLanguage.Id
            };

            var usDomain = new UmbracoDomain("/en")
            {
                RootContentId = childNode.Id,
                LanguageId = usLanguage.Id
            };

            domainService.Save(dkDomain);
            domainService.Save(usDomain);

            var pathBase = string.Empty;
            var url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null), pathBase);

            ContentItemSave model = new ContentItemSaveBuilder()
                .WithContent(grandChild)
                .WithAction(ContentSaveAction.Publish)
                .Build();

            HttpResponseMessage response = await Client.PostAsync(url, new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(model)), "contentItem" }
            });

            var body = await response.Content.ReadAsStringAsync();
            body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);
            ContentItemDisplay display = JsonConvert.DeserializeObject<ContentItemDisplay>(body);

            Assert.Multiple(() =>
            {
                Assert.NotNull(display);
                // Assert all is good, a success notification for each culture published and no warnings.
                Assert.AreEqual(2, display.Notifications.Count(x => x.NotificationType == NotificationStyle.Success));
                Assert.AreEqual(0, display.Notifications.Count(x => x.NotificationType == NotificationStyle.Warning));
            });
        }
    }
}
