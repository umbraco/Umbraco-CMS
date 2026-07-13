using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using SearchResult = Umbraco.Cms.Search.Core.Models.Searching.SearchResult;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.SearchService;

public class InvariantContentProtectionTests : SearcherTestBase
{
    public IPublicAccessService PublicAccessService => GetRequiredService<IPublicAccessService>();

    public IMemberGroupService MemberGroupService => GetRequiredService<IMemberGroupService>();

    public IMemberService MemberService => GetRequiredService<IMemberService>();

    public IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    [TestCase(true)]
    [TestCase(false)]
    public async Task CannotGetProtectedContent_Group(bool publish)
    {
        await MemberGroupService.CreateAsync(new MemberGroup() { Name = "testGroup" });

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, async () =>
        {
            await PublicAccessService.CreateAsync(
                new PublicAccessEntrySlim
                {
                    ErrorPageId = RootKey,
                    LoginPageId = RootKey,
                    ContentId = RootKey,
                    MemberGroupNames = ["testGroup"],
                });
        });


        var indexAlias = GetIndexAlias(publish);
        SearchResult results = await Searcher.SearchAsync(indexAlias, "root title", null, null, null, null, null, null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(publish ? 0 : 1));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CannotGetProtectedContent_Member(bool publish)
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
        MemberService.Save(customMember);

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, async () =>
        {
            await PublicAccessService.CreateAsync(
                new PublicAccessEntrySlim
                {
                    ErrorPageId = RootKey,
                    LoginPageId = RootKey,
                    ContentId = RootKey,
                    MemberUserNames = [customMember.Username],
                });
        });

        var indexAlias = GetIndexAlias(publish);
        SearchResult results = await Searcher.SearchAsync(indexAlias, "root title", null, null, null, null, null, null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(publish ? 0 : 1));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanGetProtectedContent_Group(bool publish)
    {
        Attempt<IMemberGroup?, MemberGroupOperationStatus> result = await MemberGroupService.CreateAsync(new MemberGroup { Name = "testGroup" });

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, async () =>
        {
            await PublicAccessService.CreateAsync(
                new PublicAccessEntrySlim
                {
                    ErrorPageId = RootKey,
                    LoginPageId = RootKey,
                    ContentId = RootKey,
                    MemberGroupNames = ["testGroup"]
                });
        });

        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
        MemberService.Save(customMember);

        var accessContext = new AccessContext(customMember.Key, [result.Result!.Key]);
        var indexAlias = GetIndexAlias(publish);
        SearchResult results = await Searcher.SearchAsync(indexAlias, "root title", null, null, null, null, null, accessContext, 0, 100);

        // We should still be able to get draft content, as it is not protected
        Assert.That(results.Total, Is.EqualTo(1));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanGetProtectedContent_Member(bool publish)
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
        MemberService.Save(customMember);

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, async () =>
        {
            await PublicAccessService.CreateAsync(
                new PublicAccessEntrySlim
                {
                    ErrorPageId = RootKey,
                    LoginPageId = RootKey,
                    ContentId = RootKey,
                    MemberUserNames = [customMember.Username],
                });
        });

        var accessContext = new AccessContext(customMember.Key, []);
        var indexAlias = GetIndexAlias(publish);
        SearchResult results = await Searcher.SearchAsync(indexAlias, "root title", null, null, null, null, null, accessContext, 0, 100);

        // We should still be able to get draft content, as it is not protected
        Assert.That(results.Total, Is.EqualTo(1));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanGetProtectedContent_IfAuthenticatedToWrongGroup(bool publish)
    {
        await MemberGroupService.CreateAsync(new MemberGroup { Name = "rightGroup" });
        Attempt<IMemberGroup?, MemberGroupOperationStatus> wrongGroupAttempt = await MemberGroupService.CreateAsync(new MemberGroup { Name = "wrongGroup" });

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, async () =>
        {
            await PublicAccessService.CreateAsync(
                new PublicAccessEntrySlim
                {
                    ErrorPageId = RootKey,
                    LoginPageId = RootKey,
                    ContentId = RootKey,
                    MemberGroupNames = ["rightGroup"],
                });
        });


        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
        MemberService.Save(customMember);

        var accessContext = new AccessContext(customMember.Key, [wrongGroupAttempt.Result!.Key]);
        var indexAlias = GetIndexAlias(publish);
        SearchResult results = await Searcher.SearchAsync(indexAlias, "root title", null, null, null, null, null, accessContext, 0, 100);

        // We should still be able to get draft content, as it is not protected
        Assert.That(results.Total, Is.EqualTo(publish ? 0 : 1));
    }

    [Test]
    public async Task CanBypassProtection()
    {
        await MemberGroupService.CreateAsync(new MemberGroup() { Name = "testGroup" });

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, async () =>
        {
            await PublicAccessService.CreateAsync(
                new PublicAccessEntrySlim
                {
                    ErrorPageId = RootKey,
                    LoginPageId = RootKey,
                    ContentId = RootKey,
                    MemberGroupNames = ["testGroup"],
                });
        });

        SearchResult results = await Searcher.SearchAsync(Cms.Core.Constants.IndexAliases.PublishedContent, "root title", null, null, null, null, null, AccessContext.BypassProtection(), 0, 100);

        Assert.That(results.Total, Is.EqualTo(1));
    }

    [SetUp]
    public async Task CreateInvariantDocument()
    {
        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        Content root = new ContentBuilder()
            .WithKey(RootKey)
            .WithContentType(contentType)
            .WithName("Root")
            .WithPropertyValues(
                new { title = "root title", })
            .Build();

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            ContentService.Save(root);
            ContentService.Publish(root, ["*"]);
            return Task.CompletedTask;
        });

        IContent? content = ContentService.GetById(RootKey);
        Assert.That(content, Is.Not.Null);
    }
}
