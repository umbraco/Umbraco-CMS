using Examine;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.IndexService;

public class MemberIndexServiceTests : IndexTestBase
{
    [Test]
    public async Task CanIndexAnyMember()
    {
        await CreateMemberAsync();
        IIndex index = GetIndex(Cms.Core.Constants.IndexAliases.DraftMembers);

        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        Assert.That(results.TotalItemCount, Is.EqualTo(1));
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

        await WaitForIndexing(Cms.Core.Constants.IndexAliases.DraftMembers, () =>
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
