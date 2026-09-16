using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DistributedContentIndexRefresherMemberTests : TestBase
{
    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IDistributedContentIndexRefresher DistributedContentIndexRefresher => GetRequiredService<IDistributedContentIndexRefresher>();

    private Guid _memberOneKey;
    private Guid _memberTwoKey;

    [SetUp]
    public async Task SetupTest()
    {
        IMemberType memberType = new MemberTypeBuilder()
            .WithAlias("invariant")
            .WithAllowAsRoot(true)
            .Build();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        _memberOneKey = Guid.NewGuid();
        IMember memberOne = new MemberBuilder()
            .WithKey(_memberOneKey)
            .WithMemberType(memberType)
            .WithName("Member one")
            .Build();
        MemberService.Save(memberOne);

        _memberTwoKey = Guid.NewGuid();
        IMember memberTwo = new MemberBuilder()
            .WithKey(_memberTwoKey)
            .WithMemberType(memberType)
            .WithName("Member two")
            .Build();
        MemberService.Save(memberTwo);

        IndexerAndSearcher.Reset();
    }

    [Test]
    public void RefreshMember_Single()
    {
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Is.Empty);

        DistributedContentIndexRefresher.RefreshMember([MemberOne()]);

        IReadOnlyList<TestIndexDocument> dump = IndexerAndSearcher.Dump(IndexAliases.Member);

        Assert.That(dump, Has.Count.EqualTo(1));
        Assert.That(dump[0].Id, Is.EqualTo(_memberOneKey));
    }

    [Test]
    public void RefreshMember_Multiple()
    {
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Is.Empty);

        DistributedContentIndexRefresher.RefreshMember([MemberOne(), MemberTwo()]);

        IReadOnlyList<TestIndexDocument> dump = IndexerAndSearcher.Dump(IndexAliases.Member);

        Assert.That(dump, Has.Count.EqualTo(2));
        Assert.That(dump.Select(d => d.Id), Is.EquivalentTo(new[] { _memberOneKey, _memberTwoKey }));
    }

    private IMember MemberOne() => MemberService.GetById(_memberOneKey) ?? throw new InvalidOperationException("Member one was not found");

    private IMember MemberTwo() => MemberService.GetById(_memberTwoKey) ?? throw new InvalidOperationException("Member two was not found");
}
