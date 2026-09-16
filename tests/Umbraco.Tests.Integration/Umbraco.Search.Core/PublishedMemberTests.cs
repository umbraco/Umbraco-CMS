using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

/// <summary>
/// Tests that member changes are handled by IPublishedContentChangeStrategy.
/// See https://github.com/umbraco/Umbraco.Cms.Search/issues/108
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class PublishedMemberTests : TestBase
{
    private const string PublishedMemberIndexAlias = "Umb_PublishedMembers_Test";

    private IContentIndexingService ContentIndexingService => GetRequiredService<IContentIndexingService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private Guid MemberOneKey { get; } = Guid.NewGuid();

    private Guid MemberTwoKey { get; } = Guid.NewGuid();

    private Guid MemberThreeKey { get; } = Guid.NewGuid();

    private IMember MemberOne() => MemberService.GetById(MemberOneKey) ?? throw new InvalidOperationException("Member one was not found");

    private IMember MemberTwo() => MemberService.GetById(MemberTwoKey) ?? throw new InvalidOperationException("Member two was not found");

    private IMember MemberThree() => MemberService.GetById(MemberThreeKey) ?? throw new InvalidOperationException("Member three was not found");

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.Services.Configure<IndexOptions>(options =>
        {
            options.RegisterContentIndex<IIndexer, ISearcher, IPublishedContentChangeStrategy>(
                PublishedMemberIndexAlias, UmbracoObjectTypes.Member);
        });
    }

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
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

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

    [Test]
    public void AllMembers_YieldsAllDocuments()
    {
        MemberService.Save([MemberOne(), MemberTwo(), MemberThree()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMemberIndexAlias);
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
    public void AllMembers_YieldsCorrectPropertyValues()
    {
        MemberService.Save([MemberOne(), MemberTwo(), MemberThree()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMemberIndexAlias);
        Assert.That(documents, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            VerifyDocumentPropertyValues(documents[0], "Organization One");
            VerifyDocumentPropertyValues(documents[1], "Organization Two");
            VerifyDocumentPropertyValues(documents[2], "Organization Three");
        });
    }

    [Test]
    public void AllMembers_YieldsCorrectStructureValues()
    {
        MemberService.Save([MemberOne(), MemberTwo(), MemberThree()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMemberIndexAlias);
        Assert.That(documents, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            VerifyDocumentStructureValues(documents[0], MemberOneKey, Guid.Empty, [MemberOneKey]);
            VerifyDocumentStructureValues(documents[1], MemberTwoKey, Guid.Empty, [MemberTwoKey]);
            VerifyDocumentStructureValues(documents[2], MemberThreeKey, Guid.Empty, [MemberThreeKey]);
        });
    }

    [Test]
    public void MemberDeleted_RemovesDocument()
    {
        MemberService.Save([MemberOne(), MemberTwo(), MemberThree()]);
        MemberService.Delete(MemberTwo());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMemberIndexAlias);
        Assert.That(documents, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(MemberOneKey));
            Assert.That(documents[1].Id, Is.EqualTo(MemberThreeKey));
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

        ContentIndexingService.Rebuild(PublishedMemberIndexAlias, DefaultOrigin);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMemberIndexAlias);
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
    public void AllMembers_DoesNotAffectDraftMemberIndex()
    {
        MemberService.Save([MemberOne(), MemberTwo(), MemberThree()]);

        // the published member index should have documents
        IReadOnlyList<TestIndexDocument> publishedDocuments = IndexerAndSearcher.Dump(PublishedMemberIndexAlias);
        Assert.That(publishedDocuments, Has.Count.EqualTo(3));

        // the draft member index should also have documents (existing behavior unchanged)
        IReadOnlyList<TestIndexDocument> draftDocuments = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(draftDocuments, Has.Count.EqualTo(3));
    }

    private void VerifyDocumentPropertyValues(TestIndexDocument document, string? organization)
    {
        var organizationValue = document.Fields.FirstOrDefault(f => f.FieldName == "organization")?.Value.Texts
            ?.SingleOrDefault();
        Assert.That(organizationValue, Is.EqualTo(organization));
    }

    private void VerifyDocumentStructureValues(TestIndexDocument document, Guid key, Guid parentKey, Guid[] pathKeys)
    {
        var idValue = document.Fields.FirstOrDefault(f => f.FieldName == Cms.Search.Core.Constants.FieldNames.Id)?.Value.Keywords?.SingleOrDefault();
        Assert.That(idValue, Is.EqualTo($"{key:D}"));

        var parentIdValue = document.Fields.FirstOrDefault(f => f.FieldName == Cms.Search.Core.Constants.FieldNames.ParentId)?.Value.Keywords?.SingleOrDefault();
        Assert.That(parentIdValue, Is.EqualTo($"{parentKey:D}"));

        var pathIdsValue = document.Fields.FirstOrDefault(f => f.FieldName == Cms.Search.Core.Constants.FieldNames.PathIds)?.Value.Keywords?.ToArray();
        Assert.That(pathIdsValue, Is.Not.Null);
        Assert.That(pathIdsValue!.Length, Is.EqualTo(pathKeys.Length));
        Assert.That(pathIdsValue, Is.EquivalentTo(pathKeys.Select(k => $"{k:D}")));
    }
}
