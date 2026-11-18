using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Verifies that element preview respects browse access and resolves picked variants for the requested culture.
/// </summary>
/// <remarks>
/// The access tests publish elements with one title and then save another so the versions are distinguishable.
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementHybridCachePreviewAccessTests : UmbracoIntegrationTest
{
    private const string PublishedTitle = "Published";
    private const string DraftTitle = "Draft";
    private const string PickerAlias = "picker";
    private const string VariantTitleAlias = "variantTitle";
    private const string English = "en-US";
    private const string Danish = "da-DK";

    private readonly TestPreviewSession _previewSession = new();

    private IContentType _elementType = null!;
    private EntityContainer _allowedFolder = null!;
    private EntityContainer _restrictedFolder = null!;
    private Guid _allowedElementKey;
    private Guid _restrictedElementKey;
    private Guid _restrictedUnpublishedElementKey;

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IPublishedElementCache PublishedElementCache => GetRequiredService<IPublishedElementCache>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Run the cache refreshers in-process so the element cache follows the element changes
        // (the harness default is a no-op messenger — see tests/Umbraco.Tests.Integration/CLAUDE.md).
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ElementTreeChangeNotification, ElementTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IPreviewSessionService>(_previewSession);
        builder.Services.AddUnique<IBackOfficeSecurityAccessor>(_previewSession);
    }

    [SetUp]
    public async Task SetUpElements()
    {
        _previewSession.User = null;
        _elementType = ContentTypeBuilder.CreateSimpleElementType("previewElement", "Preview Element");
        _elementType.AddPropertyType(new PropertyType(GetRequiredService<IShortStringHelper>(), await CreateElementPickerDataTypeAsync(), PickerAlias) { Name = "Picker" });
        await ContentTypeService.CreateAsync(_elementType, Constants.Security.SuperUserKey);

        _allowedFolder = await CreateFolderAsync();
        _restrictedFolder = await CreateFolderAsync();

        _allowedElementKey = await CreateElementAsync(_allowedFolder.Key, publish: true);
        _restrictedElementKey = await CreateElementAsync(_restrictedFolder.Key, publish: true);
        _restrictedUnpublishedElementKey = await CreateElementAsync(_restrictedFolder.Key, publish: false);
    }

    [Test]
    public async Task Preview_Shows_Draft_Element_Inside_Element_Start_Nodes()
    {
        await StartPreviewAsync(startElementId: _allowedFolder.Id);

        Assert.AreEqual(DraftTitle, GetTitle(await PublishedElementCache.GetByIdAsync(_allowedElementKey)));
    }

    [Test]
    public async Task Preview_Shows_Published_Element_Outside_Element_Start_Nodes()
    {
        await StartPreviewAsync(startElementId: _allowedFolder.Id);

        Assert.AreEqual(PublishedTitle, GetTitle(await PublishedElementCache.GetByIdAsync(_restrictedElementKey)));
        Assert.AreEqual(PublishedTitle, GetTitle(PublishedElementCache.GetById(true, _restrictedElementKey)));
    }

    [Test]
    public async Task Preview_Returns_Nothing_For_Unpublished_Element_Outside_Element_Start_Nodes()
    {
        await StartPreviewAsync(startElementId: _allowedFolder.Id);

        Assert.IsNull(await PublishedElementCache.GetByIdAsync(_restrictedUnpublishedElementKey));
    }

    [Test]
    public async Task Preview_Shows_Published_Element_Without_Browse_Permission()
    {
        await StartPreviewAsync(
            startElementId: null,
            granularPermissions:
            [
                new ElementGranularPermission { Key = _restrictedFolder.Key, Permission = ActionElementUpdate.ActionLetter },
            ]);

        Assert.AreEqual(PublishedTitle, GetTitle(await PublishedElementCache.GetByIdAsync(_restrictedElementKey)));
        Assert.AreEqual(DraftTitle, GetTitle(await PublishedElementCache.GetByIdAsync(_allowedElementKey)));
    }

    [Test]
    public async Task Preview_Shows_Published_Element_Without_Library_Section()
    {
        await StartPreviewAsync(startElementId: null, allowedSection: Constants.Applications.Content);

        Assert.AreEqual(PublishedTitle, GetTitle(await PublishedElementCache.GetByIdAsync(_allowedElementKey)));
    }

    [Test]
    public async Task Preview_Values_Resolved_For_One_Preview_User_Are_Not_Served_To_Another()
    {
        Guid pickingElementKey = await CreateElementAsync(_allowedFolder.Key, true, _restrictedElementKey);

        await StartPreviewAsync(startElementId: null);
        Assert.AreEqual(DraftTitle, GetTitle(await GetPickedElementAsync(pickingElementKey)));

        await StartPreviewAsync(startElementId: _allowedFolder.Id);
        Assert.AreEqual(PublishedTitle, GetTitle(await GetPickedElementAsync(pickingElementKey)));
    }

    [Test]
    public async Task Picker_Preview_Filters_Missing_Cultures_And_Keeps_Draft_Variants_In_Order()
    {
        Guid typeKey = await CreateVariantElementTypeAsync();
        Guid draft = await CreateVariantElementAsync(typeKey, "Draft", English, Danish);
        Guid missing = await CreateVariantElementAsync(typeKey, "Missing", English);
        Guid published = await CreateVariantElementAsync(typeKey, "Published", English, Danish);
        await PublishVariantsAsync(draft, English);
        await PublishVariantsAsync(missing, English);
        await PublishVariantsAsync(published, English, Danish);

        Guid ownerKey = await CreateElementAsync(_allowedFolder.Key, true, draft, missing, _allowedElementKey, published);
        GetRequiredService<IVariationContextAccessor>().VariationContext = new VariationContext(Danish);

        IPublishedElement? previewDraft = await PublishedElementCache.GetByIdAsync(draft, preview: true);
        IPublishedElement? publishedDraft = await PublishedElementCache.GetByIdAsync(draft, preview: false);
        Assert.That(previewDraft!.IsInvariantOrHasCulture(Danish), Is.True);
        Assert.That(publishedDraft!.IsInvariantOrHasCulture(Danish), Is.False);

        IPublishedProperty previewPicker = (await PublishedElementCache.GetByIdAsync(ownerKey, preview: true))!.GetProperty(PickerAlias)!;
        IPublishedProperty publicPicker = (await PublishedElementCache.GetByIdAsync(ownerKey, preview: false))!.GetProperty(PickerAlias)!;

        AssertPickedKeys(previewPicker, draft, _allowedElementKey, published);
        AssertPickedKeys(publicPicker, _allowedElementKey, published);

        var pickedDraft = ((IEnumerable<IPublishedElement>)previewPicker.GetValue()!).First();
        Assert.That(pickedDraft.Name, Is.EqualTo($"Draft {Danish}"));
        Assert.That(pickedDraft.GetProperty(VariantTitleAlias)!.GetValue(), Is.EqualTo($"Draft title {Danish}"));
    }

    [TestCase(Danish)]
    [TestCase("DA-dk")]
    public async Task Picker_Preview_Uses_Published_Variant_When_It_Is_Absent_From_The_Draft_Snapshot(string requestedCulture)
    {
        Guid typeKey = await CreateVariantElementTypeAsync();
        Guid elementKey = await CreateVariantElementAsync(typeKey, "Published", English, Danish);
        await PublishVariantsAsync(elementKey, English, Danish);

        IElementService elementService = GetRequiredService<IElementService>();
        IElement element = elementService.GetById(elementKey)!;
        element.SetCultureName(null, Danish);
        Assert.That(elementService.Save(element).Success, Is.True);

        Guid ownerKey = await CreateElementAsync(_allowedFolder.Key, true, _allowedElementKey, elementKey, _restrictedElementKey);
        GetRequiredService<IVariationContextAccessor>().VariationContext = new VariationContext(requestedCulture);

        IPublishedElement? preview = await PublishedElementCache.GetByIdAsync(elementKey, preview: true);
        IPublishedElement? published = await PublishedElementCache.GetByIdAsync(elementKey, preview: false);
        Assert.That(preview!.IsInvariantOrHasCulture(requestedCulture), Is.False);
        Assert.That(published!.IsInvariantOrHasCulture(requestedCulture), Is.True);

        IPublishedProperty picker = (await PublishedElementCache.GetByIdAsync(ownerKey, preview: true))!.GetProperty(PickerAlias)!;

        AssertPickedKeys(picker, _allowedElementKey, elementKey, _restrictedElementKey);
        var picked = ((IEnumerable<IPublishedElement>)picker.GetValue()!).Single(item => item.Key == elementKey);
        var apiElement = ((IEnumerable<IApiElement>)picker.GetDeliveryApiValue(expanding: false)!).Single(item => item.Id == elementKey);
        Assert.That(picked.Name, Is.EqualTo($"Published {Danish}"));
        Assert.That(picked.GetProperty(VariantTitleAlias)!.GetValue(), Is.EqualTo($"Published title {Danish}"));
        Assert.That(apiElement.Name, Is.EqualTo($"Published {Danish}"));
    }

    private static void AssertPickedKeys(IPublishedProperty picker, params Guid[] expectedKeys)
    {
        var elements = (IEnumerable<IPublishedElement>)picker.GetValue()!;
        var apiElements = (IEnumerable<IApiElement>)picker.GetDeliveryApiValue(expanding: false)!;
        var expandedApiElements = (IEnumerable<IApiElement>)picker.GetDeliveryApiValue(expanding: true)!;

        Assert.Multiple(() =>
        {
            Assert.That(elements.Select(element => element.Key), Is.EqualTo(expectedKeys));
            Assert.That(apiElements.Select(element => element.Id), Is.EqualTo(expectedKeys));
            Assert.That(expandedApiElements.Select(element => element.Id), Is.EqualTo(expectedKeys));
        });
    }

    private async Task<Guid> CreateVariantElementTypeAsync()
    {
        var language = new LanguageBuilder().WithCultureInfo(Danish).Build();
        Assert.That((await GetRequiredService<ILanguageService>().CreateAsync(language, Constants.Security.SuperUserKey)).Success, Is.True);

        var type = ContentTypeEditingBuilder.CreateContentTypeWithTwoPropertiesOneVariantAndOneInvariant(
            alias: "variantPreviewElement",
            variantPropertyAlias: VariantTitleAlias,
            isElement: true);
        var result = await GetRequiredService<IContentTypeEditingService>().CreateAsync(type, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        return result.Result.Key;
    }

    private async Task<Guid> CreateVariantElementAsync(Guid typeKey, string name, params string[] cultures)
    {
        var result = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = typeKey,
                ParentKey = _allowedFolder.Key,
                Variants = cultures.Select(culture => new VariantModel { Culture = culture, Name = $"{name} {culture}" }).ToArray(),
                Properties = cultures.Select(culture => new PropertyValueModel
                {
                    Alias = VariantTitleAlias,
                    Culture = culture,
                    Value = $"{name} title {culture}",
                }).ToArray(),
            },
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        return result.Result.Content!.Key;
    }

    private async Task PublishVariantsAsync(Guid key, params string[] cultures)
    {
        var result = await ElementPublishingService.PublishAsync(
            key,
            cultures.Select(culture => new CulturePublishScheduleModel { Culture = culture }).ToArray(),
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
    }

    private async Task<IPublishedElement?> GetPickedElementAsync(Guid pickingElementKey)
        => (await PublishedElementCache.GetByIdAsync(pickingElementKey))!
            .Value<IEnumerable<IPublishedElement>>(PickerAlias)?.SingleOrDefault();

    private async Task<IDataType> CreateElementPickerDataTypeAsync()
    {
        var dataType = new DataType(
            GetRequiredService<PropertyEditorCollection>()[Constants.PropertyEditors.Aliases.ElementPicker]!,
            GetRequiredService<IConfigurationEditorJsonSerializer>())
        {
            Name = "Preview Element Picker",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
        };

        Attempt<IDataType, DataTypeOperationStatus> attempt =
            await GetRequiredService<IDataTypeService>().CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(attempt.Success, $"Could not create the data type: {attempt.Status}");
        return attempt.Result;
    }

    private async Task<EntityContainer> CreateFolderAsync()
    {
        Attempt<EntityContainer?, EntityContainerOperationStatus> attempt =
            await ElementContainerService.CreateAsync(null, Guid.NewGuid().ToString(), null, Constants.Security.SuperUserKey);
        Assert.IsTrue(attempt.Success, $"Could not create the element folder: {attempt.Status}");
        return attempt.Result!;
    }

    private async Task<Guid> CreateElementAsync(Guid parentKey, bool publish, params Guid[] pickedKeys)
    {
        Attempt<ElementCreateResult, ContentEditingOperationStatus> createAttempt = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _elementType.Key,
                ParentKey = parentKey,
                Properties = Values(publish ? PublishedTitle : DraftTitle, pickedKeys),
                Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(createAttempt.Success, $"Could not create the element: {createAttempt.Status}");
        Guid key = createAttempt.Result.Content!.Key;

        if (publish is false)
        {
            return key;
        }

        var publishAttempt = await ElementPublishingService.PublishAsync(
            key,
            [new CulturePublishScheduleModel { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success, $"Could not publish the element: {publishAttempt.Status}");

        Attempt<ElementUpdateResult, ContentEditingOperationStatus> updateAttempt = await ElementEditingService.UpdateAsync(
            key,
            new ElementUpdateModel
            {
                Properties = Values(DraftTitle, pickedKeys),
                Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(updateAttempt.Success, $"Could not save the element draft: {updateAttempt.Status}");

        return key;
    }

    private PropertyValueModel[] Values(string title, Guid[] pickedKeys)
        => pickedKeys.Length > 0
            ? [new PropertyValueModel { Alias = "title", Value = title }, new PropertyValueModel { Alias = PickerAlias, Value = GetRequiredService<IJsonSerializer>().Serialize(pickedKeys) }]
            : [new PropertyValueModel { Alias = "title", Value = title }];

    private static string? GetTitle(IPublishedElement? element)
        => element?.Value<string>("title");

    private async Task StartPreviewAsync(
        int? startElementId,
        string allowedSection = Constants.Applications.Library,
        IList<IGranularPermission>? granularPermissions = null)
    {
        var alias = $"preview{Guid.NewGuid():N}";

        var userGroupBuilder = new UserGroupBuilder()
            .WithAlias(alias)
            .WithName(alias)
            .WithAllowedSections([allowedSection])
            .WithPermissions(new HashSet<string> { ActionElementBrowse.ActionLetter })
            .WithGranularPermissions(granularPermissions ?? []);
        if (startElementId.HasValue)
        {
            userGroupBuilder.WithStartElementId(startElementId.Value);
        }

        IUserGroup userGroup = userGroupBuilder.Build();
        userGroup.HasAccessToAllLanguages = true;

        Attempt<IUserGroup, UserGroupOperationStatus> userGroupAttempt =
            await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(userGroupAttempt.Success, $"Could not create the user group: {userGroupAttempt.Status}");

        Attempt<UserCreationResult, UserOperationStatus> userAttempt = await GetRequiredService<IUserService>().CreateAsync(
            Constants.Security.SuperUserKey,
            new UserCreateModel
            {
                Email = $"{alias}@umbraco.com",
                Name = alias,
                UserName = $"{alias}@umbraco.com",
                UserGroupKeys = new HashSet<Guid> { userGroupAttempt.Result.Key },
            },
            true);
        Assert.IsTrue(userAttempt.Success, $"Could not create the user: {userAttempt.Status}");

        _previewSession.User = await GetRequiredService<IUserService>().GetAsync(userAttempt.Result.CreatedUser!.Key);
    }

    private sealed class TestPreviewSession : IPreviewSessionService, IBackOfficeSecurityAccessor, IBackOfficeSecurity
    {
        public IUser? User { get; set; }

        public IBackOfficeSecurity? BackOfficeSecurity => this;

        public IUser? CurrentUser => User;

        public void Start()
        {
        }

        public bool IsActive() => User is not null;

        public bool UserHasSectionAccess(string section, IUser user) => user.AllowedSections.Contains(section);

        public bool IsAuthenticated() => User is not null;
    }
}
