// Copyright (c) Umbraco.
// See LICENSE for more details.
//
// These tests require registering IContentTypeFilter implementations at DI build time,
// which cannot be done on a shared host. They stay on the old per-test host boot pattern.
//
// Future opportunity: These could benefit from the snapshot pattern if we add a
// runtime-switchable composite IContentTypeFilter registered at host construction time,
// allowing per-test toggling without DI rebuild.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Filters;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
public class ContentEditingServiceFilterTests : UmbracoIntegrationTestWithContent
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    #region Root filter tests

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToAllowTextPageAtRoot))]
    public async Task Can_Create_At_Root_With_Content_Type_Filter() =>
        await Test_Can_Create_At_Root(true, true);

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToDisallowTextPageAtRoot))]
    public async Task Cannot_Create_At_Root_With_Content_Type_Filter() =>
        await Test_Can_Create_At_Root(true, false);

    public static void ConfigureContentTypeFilterToAllowTextPageAtRoot(IUmbracoBuilder builder)
        => builder.ContentTypeFilters()
            .Append<ContentTypeFilterForAllowedTextPageAtRoot>();

    public static void ConfigureContentTypeFilterToDisallowTextPageAtRoot(IUmbracoBuilder builder)
        => builder.ContentTypeFilters()
            .Append<ContentTypeFilterForDisallowedTextPageAtRoot>();

    private class ContentTypeFilterForAllowedTextPageAtRoot : ContentTypeFilterForTextPageAtRoot
    {
        public ContentTypeFilterForAllowedTextPageAtRoot() : base(true) { }
    }

    private class ContentTypeFilterForDisallowedTextPageAtRoot : ContentTypeFilterForTextPageAtRoot
    {
        public ContentTypeFilterForDisallowedTextPageAtRoot() : base(false) { }
    }

    private abstract class ContentTypeFilterForTextPageAtRoot : IContentTypeFilter
    {
        private readonly bool _allowed;

        protected ContentTypeFilterForTextPageAtRoot(bool allowed) => _allowed = allowed;

        public Task<IEnumerable<TItem>> FilterAllowedAtRootAsync<TItem>(IEnumerable<TItem> contentTypes)
            where TItem : IContentTypeComposition
            => Task.FromResult(contentTypes.Where(x => (_allowed && x.Alias == "textPage") || (!_allowed && x.Alias != "textPage")));
    }

    private async Task Test_Can_Create_At_Root(bool allowedAtRoot, bool expectSuccess)
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        contentType.AllowedAsRoot = allowedAtRoot;
        ContentTypeService.Save(contentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = "Test Create" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "bodyText", Value = "The body text" }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        if (expectSuccess)
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        }
        else
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
        }
    }

    #endregion

    #region Child filter tests

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToAllowTextPageAsChild))]
    public async Task Can_Create_As_Child_With_Content_Type_Filter() =>
        await Test_Can_Create_As_Child(true, true);

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureContentTypeFilterToDisallowTextPageAsChild))]
    public async Task Cannot_Create_As_Child_With_Content_Type_Filter() =>
        await Test_Can_Create_As_Child(true, false);

    public static void ConfigureContentTypeFilterToAllowTextPageAsChild(IUmbracoBuilder builder)
        => builder.ContentTypeFilters()
            .Append<ContentTypeFilterForAllowedTextPageAsChild>();

    public static void ConfigureContentTypeFilterToDisallowTextPageAsChild(IUmbracoBuilder builder)
        => builder.ContentTypeFilters()
            .Append<ContentTypeFilterForDisallowedTextPageAsChild>();

    private class ContentTypeFilterForAllowedTextPageAsChild : ContentTypeFilterForTextPageAsChild
    {
        public ContentTypeFilterForAllowedTextPageAsChild() : base(true) { }
    }

    private class ContentTypeFilterForDisallowedTextPageAsChild : ContentTypeFilterForTextPageAsChild
    {
        public ContentTypeFilterForDisallowedTextPageAsChild() : base(false) { }
    }

    private abstract class ContentTypeFilterForTextPageAsChild : IContentTypeFilter
    {
        private readonly bool _allowed;

        protected ContentTypeFilterForTextPageAsChild(bool allowed) => _allowed = allowed;

        public Task<IEnumerable<ContentTypeSort>> FilterAllowedChildrenAsync(IEnumerable<ContentTypeSort> contentTypes, Guid parentContentTypeKey, Guid? parentContentKey)
            => Task.FromResult(contentTypes.Where(x => (_allowed && x.Alias == "textPage") || (!_allowed && x.Alias != "textPage")));
    }

    private async Task Test_Can_Create_As_Child(bool allowedAsChild, bool expectSuccess)
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var childContentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        childContentType.AllowedAsRoot = false;
        ContentTypeService.Save(childContentType);

        var rootContentType = ContentTypeBuilder.CreateBasicContentType();
        rootContentType.AllowedAsRoot = true;
        if (allowedAsChild)
        {
            rootContentType.AllowedContentTypes = new[]
            {
                new ContentTypeSort(childContentType.Key, 1, childContentType.Alias)
            };
        }
        ContentTypeService.Save(rootContentType);

        var rootKey = (await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = rootContentType.Key,
                ParentKey = Constants.System.RootKey,
                Variants = [new VariantModel { Name = "Root" }],
            },
            Constants.Security.SuperUserKey)).Result.Content!.Key;

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = childContentType.Key,
            TemplateKey = template.Key,
            ParentKey = rootKey,
            Variants = [new VariantModel { Name = "Test Create Child" }],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The child title value" },
                new PropertyValueModel { Alias = "bodyText", Value = "The child body text" }
            ]
        };

        var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        if (expectSuccess)
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        }
        else
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
        }
    }

    #endregion
}
