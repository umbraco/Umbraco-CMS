using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.BackOffice;

public class IndexedEntitySearchServiceWithUserStartNodesTests : BackOfficeTestBase
{
    private bool _fixtureIsInitialized;

    private static Guid LimitedUserKey { get; } = new Guid("417FCE43-31EF-434D-BE76-279497F2302F");

    private IIndexedEntitySearchService IndexedEntitySearchService => GetRequiredService<IIndexedEntitySearchService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        var backOfficeSecurity = new Mock<IBackOfficeSecurity>();
        backOfficeSecurity.Setup(b => b.IsAuthenticated()).Returns(true);
        backOfficeSecurity
            .SetupGet(b => b.CurrentUser)
            .Returns(() => GetRequiredService<IUserService>().GetAsync(LimitedUserKey).GetAwaiter().GetResult());

        var backOfficeSecurityAccessor = new Mock<IBackOfficeSecurityAccessor>();
        backOfficeSecurityAccessor
            .SetupGet(b => b.BackOfficeSecurity)
            .Returns(backOfficeSecurity.Object);

        builder.Services.AddUnique(backOfficeSecurityAccessor.Object);
    }

    public override async Task SetupTest()
    {
        await base.SetupTest();

        if (_fixtureIsInitialized)
        {
            return;
        }

        IContent[] contentAtRoot = ContentService.GetRootContent().OrderBy(content => content.SortOrder).ToArray();
        ContentService.MoveToRecycleBin(contentAtRoot.Last());

        IMedia[] mediaAtRoot = MediaService.GetRootMedia().OrderBy(media => media.SortOrder).ToArray();
        MediaService.MoveToRecycleBin(mediaAtRoot.Last());

        User limitedUser = new UserBuilder()
            .WithKey(LimitedUserKey)
            .WithEmail("limited@local")
            .WithName("Limited User")
            .WithStartContentId(ContentService.GetRootContent().First().Id)
            .WithStartMediaId(MediaService.GetRootMedia().First().Id)
            .Build();
        GetRequiredService<IUserService>().Save(limitedUser);

        Attempt<UserGroupOperationStatus> groupAttempt = await GetRequiredService<IUserGroupService>().AddUsersToUserGroupAsync(
            new UsersToUserGroupManipulationModel(Constants.Security.EditorGroupKey, [limitedUser.Key]),
            Constants.Security.SuperUserKey);

        Assert.That(groupAttempt.Success, Is.True);

        _fixtureIsInitialized = true;
    }

    [TestCase("single0root", false, 1)]
    [TestCase("single0root", true, 0)]
    [TestCase("single1root", false, 0)]
    [TestCase("single1root", true, 0)]
    [TestCase("single2root", false, 0)]
    [TestCase("single2root", true, 0)]
    [TestCase("single0child", false, 1)]
    [TestCase("single0child", true, 0)]
    [TestCase("single1child", false, 1)]
    [TestCase("single1child", true, 0)]
    [TestCase("single2child", false, 1)]
    [TestCase("single2child", true, 0)]
    public async Task Content_RespectsUserStartNodes(string query, bool trashed, int expectedTotal)
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Document,
            query: query,
            parentId: null,
            contentTypeIds: null,
            trashed: trashed);

        Assert.That(result.Total, Is.EqualTo(expectedTotal));
    }

    [TestCase("single0root", false, 1)]
    [TestCase("single0root", true, 0)]
    [TestCase("single1root", false, 0)]
    [TestCase("single1root", true, 0)]
    [TestCase("single2root", false, 0)]
    [TestCase("single2root", true, 0)]
    [TestCase("single0child", false, 1)]
    [TestCase("single0child", true, 0)]
    [TestCase("single1child", false, 1)]
    [TestCase("single1child", true, 0)]
    [TestCase("single2child", false, 1)]
    [TestCase("single2child", true, 0)]
    public async Task Media_RespectsUserStartNodes(string query, bool trashed, int expectedTotal)
    {
        PagedModel<IEntitySlim> result = await IndexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Media,
            query: query,
            parentId: null,
            contentTypeIds: null,
            trashed: trashed);

        Assert.That(result.Total, Is.EqualTo(expectedTotal));
    }
}
