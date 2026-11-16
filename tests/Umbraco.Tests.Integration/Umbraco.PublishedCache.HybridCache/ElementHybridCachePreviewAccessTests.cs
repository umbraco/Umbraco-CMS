using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Asserts that a preview session only exposes the unpublished version of elements its user may browse,
/// and resolves every other element as it would outside preview.
/// </summary>
/// <remarks>
/// Each element is published with one title and then saved with another, so a draft and a published
/// version are distinguishable in every assertion.
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementHybridCachePreviewAccessTests : UmbracoIntegrationTest
{
    private const string PublishedTitle = "Published";
    private const string DraftTitle = "Draft";
    private const string PickerAlias = "picker";

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
        builder.Services.AddUnique<IPreviewSessionService>(_previewSession);
        builder.Services.AddUnique<IBackOfficeSecurityAccessor>(_previewSession);
    }

    [SetUp]
    public async Task SetUpElements()
    {
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
        Guid pickingElementKey = await CreateElementAsync(_allowedFolder.Key, publish: true, pickedKey: _restrictedElementKey);

        await StartPreviewAsync(startElementId: null);
        Assert.AreEqual(DraftTitle, GetTitle(await GetPickedElementAsync(pickingElementKey)));

        await StartPreviewAsync(startElementId: _allowedFolder.Id);
        Assert.AreEqual(PublishedTitle, GetTitle(await GetPickedElementAsync(pickingElementKey)));
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

    private async Task<Guid> CreateElementAsync(Guid parentKey, bool publish, Guid? pickedKey = null)
    {
        Attempt<ElementCreateResult, ContentEditingOperationStatus> createAttempt = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = _elementType.Key,
                ParentKey = parentKey,
                Properties = Values(publish ? PublishedTitle : DraftTitle, pickedKey),
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
                Properties = Values(DraftTitle, pickedKey),
                Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(updateAttempt.Success, $"Could not save the element draft: {updateAttempt.Status}");

        return key;
    }

    private static PropertyValueModel[] Values(string title, Guid? pickedKey)
        => pickedKey.HasValue
            ? [new PropertyValueModel { Alias = "title", Value = title }, new PropertyValueModel { Alias = PickerAlias, Value = $"[\"{pickedKey.Value}\"]" }]
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
