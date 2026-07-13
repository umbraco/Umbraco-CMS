using Umbraco.Cms.Core;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class MemberTests : ContentBaseTestBase
{
    private IContentIndexingService ContentIndexingService => GetRequiredService<IContentIndexingService>();

    [Test]
    public void AllMembers_YieldsAllDocuments()
    {
        MemberService.Save([MemberOne(), MemberTwo(), MemberThree()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(MemberOneKey));
            Assert.That(documents[1].Id, Is.EqualTo(MemberTwoKey));
            Assert.That(documents[2].Id, Is.EqualTo(MemberThreeKey));

            Assert.That(documents.All(d => d.ObjectType is UmbracoObjectTypes.Member), Is.True);
        });
    }

    [Test]
    public void AllMembers_YieldsCorrectStructureValues()
    {
        MemberService.Save([MemberOne(), MemberTwo(), MemberThree()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            VerifyDocumentStructureValues(documents[0], MemberOneKey, Guid.Empty, [MemberOneKey]);
            VerifyDocumentStructureValues(documents[1], MemberTwoKey, Guid.Empty, [MemberTwoKey]);
            VerifyDocumentStructureValues(documents[2], MemberThreeKey, Guid.Empty, [MemberThreeKey]);
        });
    }

    [Test]
    public void AllMembers_YieldsCorrectSystemValues()
    {
        MemberService.Save([MemberOne(), MemberTwo(), MemberThree()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            VerifyDocumentSystemValues(documents[0], MemberOne(), ["tag1", "tag2"]);
            VerifyDocumentSystemValues(documents[1], MemberTwo(), ["tag3", "tag4"]);
            VerifyDocumentSystemValues(documents[2], MemberThree(), ["tag5", "tag6"]);
        });
    }

    [Test]
    public void AllMembers_YieldsCorrectPropertyValues()
    {
        MemberService.Save([MemberOne(), MemberTwo(), MemberThree()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            VerifyDocumentPropertyValues(documents[0], "Organization One");
            VerifyDocumentPropertyValues(documents[1], "Organization Two");
            VerifyDocumentPropertyValues(documents[2], "Organization Three");
        });
    }

    [Test]
    public async Task AllMembers_DoesNotIndexSensitiveProperties()
    {
        IMemberTypeService memberTypeService = GetRequiredService<IMemberTypeService>();
        IMemberType? memberType = memberTypeService.Get("myMemberType");
        Assert.That(memberType, Is.Not.Null);

        memberType.SetIsSensitiveProperty("organization", true);
        await memberTypeService.UpdateAsync(memberType, Constants.Security.SuperUserKey);

        MemberService.Save([MemberOne(), MemberTwo(), MemberThree()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Fields.Any(field => field.FieldName == "organization"), Is.False);
            Assert.That(documents[1].Fields.Any(field => field.FieldName == "organization"), Is.False);
            Assert.That(documents[2].Fields.Any(field => field.FieldName == "organization"), Is.False);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void AllMembers_RebuildIndexYieldsAllDocuments(bool populateIndexBeforeRebuild)
    {
        if (populateIndexBeforeRebuild)
        {
            MemberService.Save([MemberOne(), MemberTwo(), MemberThree()]);
        }

        ContentIndexingService.Rebuild(IndexAliases.Member, DefaultOrigin);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            Assert.That(
                documents.Select(d => d.Id),
                Is.EquivalentTo(new[] { MemberOneKey, MemberTwoKey, MemberThreeKey }));

            Assert.That(documents.All(d => d.ObjectType is UmbracoObjectTypes.Member), Is.True);
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentPropertyValues(documents.Single(d => d.Id == MemberOneKey), "Organization One");
            VerifyDocumentPropertyValues(documents.Single(d => d.Id == MemberTwoKey), "Organization Two");
            VerifyDocumentPropertyValues(documents.Single(d => d.Id == MemberThreeKey), "Organization Three");
        });
    }

    [Test]
    public async Task DeleteMemberType_RemovesDocuments()
    {
        MemberService.Save([MemberOne(), MemberTwo(), MemberThree()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(3));

        await MemberTypeService.DeleteAsync(MemberOne().ContentType.Key, Constants.Security.SuperUserKey);

        documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(0));
    }

    [Test]
    public void Can_Index_More_Than_Content_Enumeration_Page_Size()
    {
        IMemberType memberType = MemberTypeService.GetAll().First();
        foreach (var count in Enumerable.Range(1, ContentChangeStrategyBase.ContentEnumerationPageSize))
        {
            MemberService.Save(
                new MemberBuilder()
                    .WithKey(Guid.NewGuid())
                    .WithMemberType(memberType)
                    .WithName($"Member {count:0000}")
                    .WithEmail($"member{count:0000}@local")
                    .WithLogin($"member{count:0000}@local", "Test123456")
                    .Build());
        }

        IndexerAndSearcher.Reset();

        ContentIndexingService.Rebuild(IndexAliases.Member, DefaultOrigin);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(ContentChangeStrategyBase.ContentEnumerationPageSize + 3));
    }

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private Guid MemberOneKey { get; } = Guid.NewGuid();

    private Guid MemberTwoKey { get; } = Guid.NewGuid();

    private Guid MemberThreeKey { get; } = Guid.NewGuid();

    private IMember MemberOne() => MemberService.GetById(MemberOneKey) ?? throw new InvalidOperationException("Member one was not found");

    private IMember MemberTwo() => MemberService.GetById(MemberTwoKey) ?? throw new InvalidOperationException("Member two was not found");

    private IMember MemberThree() => MemberService.GetById(MemberThreeKey) ?? throw new InvalidOperationException("Member three was not found");

    [SetUp]
    public async Task SetupTest()
    {
        IMemberType memberType = new MemberTypeBuilder()
            .WithAlias("myMemberType")
            .AddPropertyGroup()
            .WithName("Group")
            .AddPropertyType()
            .WithAlias("organization")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("tags")
            .WithDataTypeId(Constants.DataTypes.Tags)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Tags)
            .Done()
            .Done()
            .Build();
        await GetRequiredService<IMemberTypeService>().CreateAsync(memberType, Constants.Security.SuperUserKey);

        MemberService.Save(
            new MemberBuilder()
                .WithKey(MemberOneKey)
                .WithMemberType(memberType)
                .WithName("Member One")
                .WithEmail("memberone@local")
                .WithLogin("memberone@local", "Test123456")
                .AddPropertyData()
                .WithKeyValue("organization", "Organization One")
                .WithKeyValue("tags", "[\"tag1\",\"tag2\"]")
                .Done()
                .Build());

        MemberService.Save(
            new MemberBuilder()
                .WithKey(MemberTwoKey)
                .WithMemberType(memberType)
                .WithName("Member Two")
                .WithEmail("membertwo@local")
                .WithLogin("membertwo@local", "Test123456")
                .AddPropertyData()
                .WithKeyValue("organization", "Organization Two")
                .WithKeyValue("tags", "[\"tag3\",\"tag4\"]")
                .Done()
                .Build());

        MemberService.Save(
            new MemberBuilder()
                .WithKey(MemberThreeKey)
                .WithMemberType(memberType)
                .WithName("Member Three")
                .WithEmail("memberthree@local")
                .WithLogin("memberthree@local", "Test123456")
                .AddPropertyData()
                .WithKeyValue("organization", "Organization Three")
                .WithKeyValue("tags", "[\"tag5\",\"tag6\"]")
                .Done()
                .Build());

        IndexerAndSearcher.Reset();
    }

    private void VerifyDocumentPropertyValues(TestIndexDocument document, string? organization)
    {
        var organizationValue = document.Fields.FirstOrDefault(f => f.FieldName == "organization")?.Value.Texts
            ?.SingleOrDefault();
        Assert.That(organizationValue, Is.EqualTo(organization));
    }
}
