using Examine;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Constants = Umbraco.Cms.Search.Provider.Examine.Constants;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.IndexService;

public class InvariantDocumentProtectionIndexTests : IndexTestBase
{
    private IPublicAccessService PublicAccessService => GetRequiredService<IPublicAccessService>();

    private IMemberGroupService MemberGroupService => GetRequiredService<IMemberGroupService>();

    [Test]
    public async Task CanIndexContentProtection()
    {
        Attempt<IMemberGroup?, MemberGroupOperationStatus> result = await MemberGroupService.CreateAsync(new MemberGroup { Name = "testGroup" });

        await WaitForIndexing(Cms.Search.Core.Constants.IndexAliases.PublishedContent, async () =>
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

        IIndex index = GetIndex(Cms.Search.Core.Constants.IndexAliases.PublishedContent);
        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        IReadOnlyList<string> indexedAccessKeys = results.First().AllValues.First(x => x.Key == Constants.SystemFields.Protection).Value;
        Assert.That(indexedAccessKeys, Has.Count.EqualTo(1));
        Assert.That(indexedAccessKeys, Has.Member(result.Result!.Key.AsKeyword()));
    }

    [Test]
    public async Task CanIndexMultipleContentProtection()
    {
        Attempt<IMemberGroup?, MemberGroupOperationStatus> group = await MemberGroupService.CreateAsync(new MemberGroup { Name = "testGroup" });
        Attempt<IMemberGroup?, MemberGroupOperationStatus> group2 = await MemberGroupService.CreateAsync(new MemberGroup { Name = "testGroup 2" });
        Attempt<IMemberGroup?, MemberGroupOperationStatus> group3 = await MemberGroupService.CreateAsync(new MemberGroup { Name = "testGroup 3" });
        Attempt<IMemberGroup?, MemberGroupOperationStatus> group4 = await MemberGroupService.CreateAsync(new MemberGroup { Name = "testGroup 4" });
        Attempt<IMemberGroup?, MemberGroupOperationStatus> group5 = await MemberGroupService.CreateAsync(new MemberGroup { Name = "testGroup 5" });

        await WaitForIndexing(Cms.Search.Core.Constants.IndexAliases.PublishedContent, async () =>
        {
            await PublicAccessService.CreateAsync(
                new PublicAccessEntrySlim
                {
                    ErrorPageId = RootKey,
                    LoginPageId = RootKey,
                    ContentId = RootKey,
                    MemberGroupNames = ["testGroup", "testGroup 2", "testGroup 3", "testGroup 4", "testGroup 5"],
                });
        });

        IIndex index = GetIndex(Cms.Search.Core.Constants.IndexAliases.PublishedContent);
        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        IReadOnlyList<string> indexedAccessKeys = results.First().AllValues.First(x => x.Key == Constants.SystemFields.Protection).Value;
        Assert.That(indexedAccessKeys, Has.Count.EqualTo(5));
        Assert.That(indexedAccessKeys, Has.Member(group.Result!.Key.AsKeyword()));
        Assert.That(indexedAccessKeys, Has.Member(group2.Result!.Key.AsKeyword()));
        Assert.That(indexedAccessKeys, Has.Member(group3.Result!.Key.AsKeyword()));
        Assert.That(indexedAccessKeys, Has.Member(group4.Result!.Key.AsKeyword()));
        Assert.That(indexedAccessKeys, Has.Member(group5.Result!.Key.AsKeyword()));
    }

    [Test]
    public void DoesNotIndexContentProtectionIfNoneExists()
    {
        IIndex index = GetIndex(Cms.Search.Core.Constants.IndexAliases.PublishedContent);
        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        Assert.That(results.First().AllValues.SelectMany(x => x.Value), Does.Not.Contain(Constants.SystemFields.Protection));
    }

    [SetUp]
    public async Task CreateInvariantDocument()
    {
        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Umbraco.Cms.Core.Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Umbraco.Cms.Core.Constants.Security.SuperUserKey);

        Content root = new ContentBuilder()
            .WithKey(RootKey)
            .WithContentType(contentType)
            .WithName("Root")
            .WithPropertyValues(
                new
                {
                    title = "The root title",
                })
            .Build();

        await WaitForIndexing(Cms.Search.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            SaveAndPublish(root);
            return Task.CompletedTask;
        });

        IContent? content = ContentService.GetById(RootKey);
        Assert.That(content, Is.Not.Null);
    }
}
