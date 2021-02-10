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
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Formatters;
using Umbraco.Extensions;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Integration.TestServerTest;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Tests.Integration.Umbraco.Web.BackOffice.Controllers
{
    [TestFixture]
    public class ContentControllerTests : UmbracoTestServerTestBase
    {
        /// <summary>
        ///     Returns 404 if the content wasn't found based on the ID specified
        /// </summary>
        [Test]
        public async Task PostSave_Validate_Existing_Content()
        {
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();

            // Add another language
            localizationService.Save(new LanguageBuilder()
                .WithCultureInfo("da-DK")
                .WithIsDefault(false)
                .Build());

            string url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

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
                .WithCultureInfo("da-DK")
                .WithIsDefault(false)
                .Build());

            string url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

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
                .WithCultureInfo("da-DK")
                .WithIsDefault(false)
                .Build());

            string url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

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
                .WithCultureInfo("da-DK")
                .WithIsDefault(false)
                .Build());

            string url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

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
                .WithCultureInfo("da-DK")
                .WithIsDefault(false)
                .Build());

            string url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

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
                .WithCultureInfo("da-DK")
                .WithIsDefault(false)
                .Build());

            string url = PrepareApiControllerUrl<ContentController>(x => x.PostSave(null));

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
                .WithCultureName("en-US", "English")
                .WithCultureName("da-DK", "Danish")
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
    }
}
