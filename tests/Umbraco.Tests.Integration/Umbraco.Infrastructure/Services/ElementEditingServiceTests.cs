using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
/// NOTE: ElementEditingService and ContentEditingService share most of their implementation.
///
///       as such, these tests for ElementEditingService are not exhaustive, because that would require too much
///       duplication from the ContentEditingService tests, without any real added value.
///
///       instead, these tests focus on validating that the most basic functionality is in place for element editing.
/// </summary>
[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
public partial class ElementEditingServiceTests : UmbracoIntegrationTest
{
    [SetUp]
    public void Setup() => ContentRepositoryBase.ThrowOnWarning = true;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder
        .AddNotificationHandler<ElementUnpublishingNotification, ElementNotificationHandler>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    private async Task<IContentType> CreateInvariantElementType(bool allowedAtRoot = true)
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias("invariantTest")
            .WithName("Invariant Test")
            .WithAllowAsRoot(allowedAtRoot)
            .WithIsElement(true)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .Done()
            .AddPropertyType()
            .WithAlias("text")
            .WithName("Text")
            .Done()
            .Build();

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }

    private async Task<IContentType> CreateVariantElementType(ContentVariation variation = ContentVariation.Culture, bool variantTitleAsMandatory = true, bool allowedAtRoot = true)
    {
        var language = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);

        var elementType = new ContentTypeBuilder()
            .WithAlias("cultureVariationTest")
            .WithName("Culture Variation Test")
            .WithAllowAsRoot(allowedAtRoot)
            .WithIsElement(true)
            .WithContentVariation(variation)
            .AddPropertyType()
            .WithAlias("variantTitle")
            .WithName("Variant Title")
            .WithMandatory(variantTitleAsMandatory)
            .WithVariations(variation)
            .Done()
            .AddPropertyType()
            .WithAlias("invariantTitle")
            .WithName("Invariant Title")
            .WithVariations(ContentVariation.Nothing)
            .Done()
            .AddPropertyType()
            .WithAlias("variantLabel")
            .WithName("Variant Label")
            .WithDataTypeId(Constants.DataTypes.LabelString)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .WithVariations(variation)
            .Done()
            .Build();

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        return elementType;
    }

    private async Task<IElement> CreateInvariantElement(Guid? parentKey = null)
    {
        var elementType = await CreateInvariantElementType();

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = parentKey,
            Variants =
            [
                new VariantModel { Name = "Initial Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The initial title" },
                new PropertyValueModel { Alias = "text", Value = "The initial text" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    private async Task<IElement> CreateCultureVariantElement(Guid? parentKey = null)
    {
        var elementType = await CreateVariantElementType();

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = parentKey,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The initial invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial Danish title", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Initial English Name" },
                new VariantModel { Culture = "da-DK", Name = "Initial Danish Name" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    private async Task<IElement> CreateSegmentVariantElement(Guid? parentKey = null)
    {
        var elementType = await CreateVariantElementType(ContentVariation.Segment);

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = parentKey,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The initial invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial default title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial seg-1 title", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial seg-2 title", Segment = "seg-2" }
            ],
            Variants =
            [
                new VariantModel { Segment = null, Name = "The Name" },
                new VariantModel { Segment = "seg-1", Name = "The Name" },
                new VariantModel { Segment = "seg-2", Name = "The Name" }
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    private async Task<IElement> CreateCultureAndSegmentVariantElement(Guid? parentKey = null)
    {
        var elementType = await CreateVariantElementType(ContentVariation.CultureAndSegment);

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = parentKey,
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The initial invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial title in English", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial seg-1 title in English", Culture = "en-US", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial seg-2 title in English", Culture = "en-US", Segment = "seg-2" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial title in Danish", Culture = "da-DK" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial seg-1 title in Danish", Culture = "da-DK", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial seg-2 title in Danish", Culture = "da-DK", Segment = "seg-2" }
            ],
            Variants =
            [
                new VariantModel { Name = "The Name", Culture = "en-US", Segment = null },
                new VariantModel { Name = "The Name", Culture = "en-US", Segment = "seg-1" },
                new VariantModel { Name = "The Name", Culture = "en-US", Segment = "seg-2" },
                new VariantModel { Name = "The Name", Culture = "da-DK", Segment = null },
                new VariantModel { Name = "The Name", Culture = "da-DK", Segment = "seg-1" },
                new VariantModel { Name = "The Name", Culture = "da-DK", Segment = "seg-2" },
            ],
        };

        var result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        return result.Result.Content!;
    }

    private IEntitySlim[] GetFolderChildren(Guid containerKey, bool trashed = false)
        => EntityService.GetPagedChildren(containerKey, [UmbracoObjectTypes.ElementContainer], [UmbracoObjectTypes.ElementContainer, UmbracoObjectTypes.Element], 0, 999, trashed, out _).ToArray();

    internal sealed class ElementNotificationHandler : INotificationHandler<ElementUnpublishingNotification>
    {
        public static Action<ElementUnpublishingNotification>? UnpublishingElement { get; set; }

        public void Handle(ElementUnpublishingNotification notification) => UnpublishingElement?.Invoke(notification);
    }
}
