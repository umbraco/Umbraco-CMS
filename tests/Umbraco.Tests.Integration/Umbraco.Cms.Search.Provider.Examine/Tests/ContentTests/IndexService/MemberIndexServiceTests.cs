using Examine;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.IndexService;

[LongRunning]
public class MemberIndexServiceTests : IndexTestBase
{
    [Test]
    public async Task CanIndexAnyMember()
    {
        await CreateMemberAsync();
        IIndex index = GetIndex(Constants.Search.IndexAliases.DraftMembers);

        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        Assert.That(results.TotalItemCount, Is.EqualTo(1));
    }

    [Test]
    public async Task CanRemoveDeletedExternalMember()
    {
        IExternalMemberService externalMemberService = GetRequiredService<IExternalMemberService>();
        IIndex index = GetIndex(Constants.Search.IndexAliases.DraftMembers);

        ExternalMemberIdentity? member = null;
        await WaitForIndexing(Constants.Search.IndexAliases.DraftMembers, async () =>
        {
            Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus> result = await externalMemberService.CreateAsync(
                new ExternalMemberIdentity { Email = "external@local", UserName = "external@local", Name = "The External Member" });
            Assert.That(result.Success, Is.True);
            member = result.Result;
        });
        Assert.That(index.Searcher.CreateQuery().All().Execute().TotalItemCount, Is.EqualTo(1));

        await WaitForIndexing(Constants.Search.IndexAliases.DraftMembers, () => externalMemberService.DeleteAsync(member!.Key));

        Assert.That(index.Searcher.CreateQuery().All().Execute().TotalItemCount, Is.EqualTo(0));
    }

    private async Task CreateMemberAsync()
    {
        IMemberType memberType = new MemberTypeBuilder()
            .WithAlias("theMemberType")
            .AddPropertyGroup()
            .AddPropertyType()
            .WithAlias("organization")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Done()
            .Build();
        await GetRequiredService<IMemberTypeService>().CreateAsync(memberType, Constants.Security.SuperUserKey);

        await WaitForIndexing(Constants.Search.IndexAliases.DraftMembers, () =>
        {
            GetRequiredService<IMemberService>().Save(
                new MemberBuilder()
                    .WithMemberType(memberType)
                    .WithName("The Member")
                    .WithEmail("member@local")
                    .WithLogin("member@local", "Test123456")
                    .AddPropertyData()
                    .WithKeyValue("organization", "The Organization")
                    .Done()
                    .Build());
            return Task.CompletedTask;
        });
    }
}
