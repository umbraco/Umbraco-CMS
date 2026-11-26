using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Search.Querying;
using Umbraco.Cms.Core.Search.Querying.Sorting;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.SearchService;

[LongRunning]
public class MemberSortingTests : SearcherTestBase
{
    [TestCase(Constants.Search.MemberFieldNames.Email, Direction.Ascending)]
    [TestCase(Constants.Search.MemberFieldNames.Email, Direction.Descending)]
    [TestCase(Constants.Search.MemberFieldNames.UserName, Direction.Ascending)]
    [TestCase(Constants.Search.MemberFieldNames.UserName, Direction.Descending)]
    public async Task CanSortByMemberField(string fieldName, Direction direction)
    {
        // creation order, name order, email order and username order all differ, and each value shares a token that
        // sorts after the distinguishing one - so a field that is merely analysed (not sortable) cannot sort them by chance
        ExternalMemberIdentity[] members = await CreateExternalMembersAsync(
            ("c@local", "b-member", "Member A"),
            ("a@local", "c-member", "Member B"),
            ("b@local", "a-member", "Member C"));

        Guid[] expectedKeys = members
            .OrderBy(member => fieldName == Constants.Search.MemberFieldNames.Email ? member.Email : member.UserName, direction)
            .Select(member => member.Key)
            .ToArray();

        SearchResult result = await Searcher.SearchAsync(
            Constants.Search.IndexAliases.DraftMembers,
            null,
            null,
            null,
            [new TextSorter(fieldName, direction)],
            null,
            null,
            null,
            0,
            100);

        Assert.That(result.Documents.Select(document => document.Id), Is.EqualTo(expectedKeys));
    }

    private async Task<ExternalMemberIdentity[]> CreateExternalMembersAsync(params (string Email, string UserName, string Name)[] values)
    {
        IExternalMemberService externalMemberService = GetRequiredService<IExternalMemberService>();
        var members = new List<ExternalMemberIdentity>();

        await WaitForIndexing(Constants.Search.IndexAliases.DraftMembers, async () =>
        {
            foreach ((string email, string userName, string name) in values)
            {
                Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus> result = await externalMemberService.CreateAsync(
                    new ExternalMemberIdentity { Email = email, UserName = userName, Name = name });
                Assert.That(result.Success, Is.True);
                members.Add(result.Result);
            }
        });

        return members.ToArray();
    }
}
