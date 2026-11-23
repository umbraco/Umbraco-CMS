using System.Security.Claims;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Asserts that a preview session only exposes the unpublished version of documents its user may browse,
/// and resolves every other document as it would outside preview.
/// </summary>
/// <remarks>
/// Each document is published with one title and then saved with another, so a draft and a published
/// version are distinguishable in every assertion.
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCachePreviewAccessTests : UmbracoIntegrationTest
{
    private const string PublishedTitle = "Published";
    private const string DraftTitle = "Draft";
    private const string PickerAlias = "picker";

    private readonly TestPreviewService _previewService = new();

    private IContentType _contentType = null!;
    private IContent _allowedRoot = null!;
    private IContent _allowedChild = null!;
    private IContent _restrictedRoot = null!;
    private IContent _restrictedChild = null!;
    private IContent _restrictedUnpublishedChild = null!;

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    private IDocumentNavigationQueryService NavigationQueryService => GetRequiredService<IDocumentNavigationQueryService>();

    private IPublishedContentStatusFilteringService FilteringService => GetRequiredService<IPublishedContentStatusFilteringService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Run the cache refreshers in-process so navigation and publish status follow the content changes
        // (the harness default is a no-op messenger — see tests/Umbraco.Tests.Integration/CLAUDE.md).
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IVariationContextAccessor, TestVariationContextAccessor>();
        builder.Services.AddUnique<IPreviewService>(_previewService);
    }

    [SetUp]
    public async Task SetUpContent()
    {
        // Traversal filters by the request culture, as it would on a front-end request.
        GetRequiredService<IVariationContextAccessor>().VariationContext = new VariationContext("en-US");

        Template template = TemplateBuilder.CreateTextPageTemplate("page");
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("page", "Page", defaultTemplateId: template.Id);
        contentType.AllowedAsRoot = true;
        IDataType contentPicker = (await GetRequiredService<IDataTypeService>().GetAsync(Constants.DataTypes.Guids.ContentPickerGuid))!;
        contentType.AddPropertyType(new PropertyType(GetRequiredService<IShortStringHelper>(), contentPicker, PickerAlias) { Name = "Picker" });
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        _contentType = contentType;

        _allowedRoot = CreatePublishedDocumentWithDraft(Constants.System.Root);
        _allowedChild = CreatePublishedDocumentWithDraft(_allowedRoot.Id);
        _restrictedRoot = CreatePublishedDocumentWithDraft(Constants.System.Root);
        _restrictedChild = CreatePublishedDocumentWithDraft(_restrictedRoot.Id);

        _restrictedUnpublishedChild = ContentBuilder.CreateSimpleContent(contentType, "Unpublished", _restrictedRoot.Id);
        _restrictedUnpublishedChild.SetValue("title", DraftTitle);
        ContentService.Save(_restrictedUnpublishedChild);
    }

    [Test]
    public async Task Preview_Shows_Draft_Inside_Start_Nodes()
    {
        await StartPreviewAsync(startContentId: _allowedRoot.Id);

        Assert.AreEqual(DraftTitle, GetTitle(PublishedContentCache.GetById(_allowedChild.Key)));
    }

    [Test]
    public async Task Preview_Shows_Published_Version_Outside_Start_Nodes()
    {
        await StartPreviewAsync(startContentId: _allowedRoot.Id);

        Assert.AreEqual(PublishedTitle, GetTitle(PublishedContentCache.GetById(_restrictedRoot.Key)));
        Assert.AreEqual(PublishedTitle, GetTitle(await PublishedContentCache.GetByIdAsync(_restrictedChild.Id)));
    }

    [Test]
    public async Task Preview_Returns_Nothing_For_Unpublished_Document_Outside_Start_Nodes()
    {
        await StartPreviewAsync(startContentId: _allowedRoot.Id);

        Assert.IsNull(PublishedContentCache.GetById(_restrictedUnpublishedChild.Key));
    }

    [Test]
    public async Task Preview_Shows_Published_Versions_When_Traversing_Outside_Start_Nodes()
    {
        await StartPreviewAsync(startContentId: _allowedRoot.Id);

        Assert.IsTrue(NavigationQueryService.TryGetChildrenKeys(_restrictedRoot.Key, out IEnumerable<Guid> childKeys));
        IPublishedContent[] children = FilteringService.FilterAvailable(childKeys, null).ToArray();

        Assert.AreEqual(1, children.Length);
        Assert.AreEqual(_restrictedChild.Key, children[0].Key);
        Assert.AreEqual(PublishedTitle, GetTitle(children[0]));
    }

    [Test]
    public async Task Preview_Shows_Drafts_When_Traversing_Inside_Start_Nodes()
    {
        await StartPreviewAsync(startContentId: _allowedRoot.Id);

        Assert.IsTrue(NavigationQueryService.TryGetChildrenKeys(_allowedRoot.Key, out IEnumerable<Guid> childKeys));
        IPublishedContent[] children = FilteringService.FilterAvailable(childKeys, null).ToArray();

        Assert.AreEqual(1, children.Length);
        Assert.AreEqual(DraftTitle, GetTitle(children[0]));
    }

    [Test]
    public async Task Preview_Shows_Published_Version_Without_Browse_Permission()
    {
        await StartPreviewAsync(
            startContentId: null,
            granularPermissions:
            [
                new DocumentGranularPermission { Key = _restrictedRoot.Key, Permission = ActionUpdate.ActionLetter },
            ]);

        Assert.AreEqual(PublishedTitle, GetTitle(PublishedContentCache.GetById(_restrictedChild.Key)));
        Assert.AreEqual(DraftTitle, GetTitle(PublishedContentCache.GetById(_allowedChild.Key)));
    }

    [Test]
    public async Task Preview_Values_Resolved_For_One_Preview_User_Are_Not_Served_To_Another()
    {
        IContent pickingPage = CreatePublishedDocumentWithDraft(
            _allowedRoot.Id,
            Udi.Create(Constants.UdiEntityType.Document, _restrictedChild.Key).ToString());

        await StartPreviewAsync(startContentId: null);
        Assert.AreEqual(DraftTitle, GetTitle(PublishedContentCache.GetById(pickingPage.Key)!.Value<IPublishedContent>(PickerAlias)));

        await StartPreviewAsync(startContentId: _allowedRoot.Id);
        Assert.AreEqual(PublishedTitle, GetTitle(PublishedContentCache.GetById(pickingPage.Key)!.Value<IPublishedContent>(PickerAlias)));
    }

    private IContent CreatePublishedDocumentWithDraft(int parentId, string? pickedUdi = null)
    {
        Content content = ContentBuilder.CreateSimpleContent(_contentType, Guid.NewGuid().ToString(), parentId);
        content.SetValue(PickerAlias, pickedUdi);
        content.SetValue("title", PublishedTitle);
        ContentService.Save(content);
        ContentService.Publish(content, ["*"]);

        content.SetValue("title", DraftTitle);
        ContentService.Save(content);

        return content;
    }

    private static string? GetTitle(IPublishedContent? content)
        => content?.Value<string>("title");

    private async Task StartPreviewAsync(
        int? startContentId,
        IList<IGranularPermission>? granularPermissions = null)
    {
        var alias = $"preview{Guid.NewGuid():N}";

        var userGroupBuilder = new UserGroupBuilder()
            .WithAlias(alias)
            .WithName(alias)
            .WithAllowedSections([Constants.Applications.Content])
            .WithPermissions(new HashSet<string> { ActionBrowse.ActionLetter })
            .WithGranularPermissions(granularPermissions ?? []);
        if (startContentId.HasValue)
        {
            userGroupBuilder.WithStartContentId(startContentId.Value);
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

        _previewService.UserKey = userAttempt.Result.CreatedUser!.Key;
    }

    private sealed class TestPreviewService : IPreviewService
    {
        public Guid? UserKey { get; set; }

        public Task<bool> TryEnterPreviewAsync(IUser user) => Task.FromResult(true);

        public Task EndPreviewAsync() => Task.CompletedTask;

        public bool IsInPreview() => UserKey.HasValue;

        public Task<Attempt<ClaimsIdentity>> TryGetPreviewClaimsIdentityAsync()
            => Task.FromResult(UserKey.HasValue
                ? Attempt.Succeed(new ClaimsIdentity(
                    [new Claim(Constants.Security.OpenIdDictSubClaimType, UserKey.Value.ToString())],
                    Constants.Security.BackOfficeAuthenticationType))
                : Attempt<ClaimsIdentity>.Fail());
    }
}
