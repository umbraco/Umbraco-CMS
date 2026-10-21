using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Search.Configuration;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Search.Core.Services.ExternalMemberIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;
using MemberFieldNames = Umbraco.Cms.Core.Constants.Search.MemberFieldNames;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class ExternalMemberTests : ContentBaseTestBase
{
    private const string PublishedMemberIndexAlias = "Umb_PublishedMembers_Test";

    private const string SameOriginOnlyMemberIndexAlias = "Umb_SameOriginOnlyMembers_Test";

    private IExternalMemberService ExternalMemberService => GetRequiredService<IExternalMemberService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IContentIndexingService ContentIndexingService => GetRequiredService<IContentIndexingService>();

    private IDateTimeOffsetConverter DateTimeOffsetConverter => GetRequiredService<IDateTimeOffsetConverter>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.Services.Configure<IndexOptions>(options =>
        {
            options.RegisterContentIndex<IIndexer, ISearcher, IPublishedContentChangeStrategy>(PublishedMemberIndexAlias, UmbracoObjectTypes.Member);
            options.RegisterContentIndex<IIndexer, ISearcher, IDraftContentChangeStrategy>(SameOriginOnlyMemberIndexAlias, true, UmbracoObjectTypes.Member);
        });
    }

    [SetUp]
    public void SetupTest() => IndexerAndSearcher.Reset();

    [Test]
    public async Task CreateExternalMember_YieldsDocumentWithSystemAndMemberFields()
    {
        ExternalMemberIdentity member = await CreateExternalMemberAsync("external-one@local", "External One");

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents[0];
        Assert.Multiple(() =>
        {
            Assert.That(document.Id, Is.EqualTo(member.Key));
            Assert.That(document.ObjectType, Is.EqualTo(UmbracoObjectTypes.Member));
            Assert.That(document.Variations.Single(), Is.EqualTo(new Variation(null, null)));

            Assert.That(Keyword(document, Constants.Search.FieldNames.Id), Is.EqualTo(member.Key.AsKeyword()));
            Assert.That(Keyword(document, Constants.Search.FieldNames.ObjectType), Is.EqualTo(nameof(UmbracoObjectTypes.Member)));
            Assert.That(Field(document, Constants.Search.FieldNames.Name)?.Value.TextsR1?.SingleOrDefault(), Is.EqualTo("External One"));
            Assert.That(Field(document, Constants.Search.FieldNames.CreateDate)?.Value.DateTimeOffsets?.SingleOrDefault(), Is.EqualTo(DateTimeOffsetConverter.ToDateTimeOffset(member.CreateDate)));
            Assert.That(Field(document, Constants.Search.FieldNames.UpdateDate)?.Value.DateTimeOffsets?.SingleOrDefault(), Is.EqualTo(DateTimeOffsetConverter.ToDateTimeOffset(member.UpdateDate)));

            Assert.That(Keyword(document, MemberFieldNames.Email), Is.EqualTo("external-one@local"));
            Assert.That(Keyword(document, MemberFieldNames.UserName), Is.EqualTo("external-one@local"));
            Assert.That(Integer(document, MemberFieldNames.IsApproved), Is.EqualTo(1));
            Assert.That(Integer(document, MemberFieldNames.IsLockedOut), Is.EqualTo(0));
            Assert.That(Integer(document, MemberFieldNames.IsExternalMember), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ContentMemberAndExternalMember_YieldSameMemberFields()
    {
        IMemberType memberType = new MemberTypeBuilder().WithAlias("contentMemberType").Build();
        await GetRequiredService<IMemberTypeService>().CreateAsync(memberType, Constants.Security.SuperUserKey);

        var contentMemberKey = Guid.NewGuid();
        MemberService.Save(
            new MemberBuilder()
                .WithKey(contentMemberKey)
                .WithMemberType(memberType)
                .WithName("Content Member")
                .WithEmail("content-member@local")
                .WithLogin("content-member-login", "Test123456")
                .WithIsApproved(true)
                .Build());

        ExternalMemberIdentity externalMember = await CreateExternalMemberAsync("external-two@local", "External Two");

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(2));

        TestIndexDocument contentDocument = documents.Single(d => d.Id == contentMemberKey);
        TestIndexDocument externalDocument = documents.Single(d => d.Id == externalMember.Key);

        Assert.Multiple(() =>
        {
            Assert.That(Keyword(contentDocument, MemberFieldNames.Email), Is.EqualTo("content-member@local"));
            Assert.That(Keyword(contentDocument, MemberFieldNames.UserName), Is.EqualTo("content-member-login"));
            Assert.That(Integer(contentDocument, MemberFieldNames.IsApproved), Is.EqualTo(1));
            Assert.That(Integer(contentDocument, MemberFieldNames.IsLockedOut), Is.EqualTo(0));
            Assert.That(Integer(contentDocument, MemberFieldNames.IsExternalMember), Is.EqualTo(0));

            Assert.That(Integer(externalDocument, MemberFieldNames.IsExternalMember), Is.EqualTo(1));
            Assert.That(Field(externalDocument, Constants.Search.FieldNames.ContentTypeId), Is.Null);
        });
    }

    [Test]
    public async Task UpdateExternalMember_UpdatesDocument()
    {
        ExternalMemberIdentity member = await CreateExternalMemberAsync("external-three@local", "External Three");
        IndexerAndSearcher.Reset();

        member.Name = "External Three Renamed";
        member.IsLockedOut = true;
        await ExternalMemberService.UpdateAsync(member);

        TestIndexDocument document = IndexerAndSearcher.Dump(IndexAliases.Member).Single();
        Assert.Multiple(() =>
        {
            Assert.That(Field(document, Constants.Search.FieldNames.Name)?.Value.TextsR1?.SingleOrDefault(), Is.EqualTo("External Three Renamed"));
            Assert.That(Integer(document, MemberFieldNames.IsLockedOut), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ExternalMemberProfileData_YieldsProfileFields()
    {
        var member = new ExternalMemberIdentity
        {
            Email = "external-profile@local",
            UserName = "external-profile@local",
            Name = "External Profile",
            ProfileData = """{"favouriteColour":"Turquoise","petCount":2,"Umb_Email":"spoofed@local"}""",
        };
        Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus> result = await ExternalMemberService.CreateAsync(member);
        Assert.That(result.Success, Is.True);

        TestIndexDocument document = IndexerAndSearcher.Dump(IndexAliases.Member).Single();
        Assert.Multiple(() =>
        {
            Assert.That(Field(document, "favouriteColour")?.Value.Texts?.SingleOrDefault(), Is.EqualTo("Turquoise"));
            Assert.That(Integer(document, "petCount"), Is.EqualTo(2));
            Assert.That(Keyword(document, MemberFieldNames.Email), Is.EqualTo("external-profile@local"));
            Assert.That(document.Fields.Count(field => field.FieldName == MemberFieldNames.Email), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task UpdateExternalMemberProfileData_UpdatesDocument()
    {
        ExternalMemberIdentity member = await CreateExternalMemberAsync("external-profile-update@local", "External Profile Update");
        IndexerAndSearcher.Reset();

        member.ProfileData = """{"petName":"Biscuit"}""";
        await ExternalMemberService.UpdateAsync(member);

        TestIndexDocument document = IndexerAndSearcher.Dump(IndexAliases.Member).Single();
        Assert.That(Field(document, "petName")?.Value.Texts?.SingleOrDefault(), Is.EqualTo("Biscuit"));
    }

    [Test]
    public async Task UpdateExternalMemberLoginProperties_DoesNotReindex()
    {
        ExternalMemberIdentity member = await CreateExternalMemberAsync("external-four@local", "External Four");
        IndexerAndSearcher.Reset();

        member.LastLoginDate = DateTime.UtcNow;
        member.SecurityStamp = Guid.NewGuid().ToString();
        await ExternalMemberService.UpdateLoginPropertiesAsync(member);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Is.Empty);
    }

    [Test]
    public async Task ConvertExternalMemberToContentMember_ReplacesDocumentWithContentMemberDocument()
    {
        IMemberType memberType = new MemberTypeBuilder().WithAlias("convertedMemberType").Build();
        await GetRequiredService<IMemberTypeService>().CreateAsync(memberType, Constants.Security.SuperUserKey);

        Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus> createResult = await ExternalMemberService.CreateAsync(
            new ExternalMemberIdentity
            {
                Email = "external-convert@local",
                UserName = "external-convert@local",
                Name = "External Convert",
                ProfileData = """{"petName":"Biscuit"}""",
            });
        Assert.That(createResult.Success, Is.True);
        Guid memberKey = createResult.Result.Key;

        TestIndexDocument externalDocument = IndexerAndSearcher.Dump(IndexAliases.Member).Single();
        Assert.Multiple(() =>
        {
            Assert.That(Integer(externalDocument, MemberFieldNames.IsExternalMember), Is.EqualTo(1));
            Assert.That(Field(externalDocument, "petName"), Is.Not.Null);
        });

        Attempt<IMember?, ExternalMemberOperationStatus> convertResult = await ExternalMemberService.ConvertToContentMemberAsync(memberKey, memberType.Alias);
        Assert.That(convertResult.Success, Is.True);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument contentDocument = documents[0];
        Assert.Multiple(() =>
        {
            Assert.That(contentDocument.Id, Is.EqualTo(memberKey));
            Assert.That(Integer(contentDocument, MemberFieldNames.IsExternalMember), Is.EqualTo(0));
            Assert.That(Keyword(contentDocument, Constants.Search.FieldNames.ContentTypeId), Is.EqualTo(memberType.Key.AsKeyword()));
            Assert.That(Keyword(contentDocument, MemberFieldNames.Email), Is.EqualTo("external-convert@local"));
            Assert.That(Field(contentDocument, "petName"), Is.Null);
        });
    }

    [Test]
    public async Task DeleteExternalMember_RemovesDocument()
    {
        ExternalMemberIdentity member = await CreateExternalMemberAsync("external-five@local", "External Five");
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Has.Count.EqualTo(1));

        await ExternalMemberService.DeleteAsync(member.Key);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Is.Empty);
    }

    [Test]
    public async Task ExternalMemberChanges_ApplyToEveryIndexContainingMembers_RegardlessOfChangeStrategy()
    {
        ExternalMemberIdentity member = await CreateExternalMemberAsync("external-six@local", "External Six");

        Assert.Multiple(() =>
        {
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member).Select(d => d.Id), Is.EqualTo(new[] { member.Key }));
            Assert.That(IndexerAndSearcher.Dump(PublishedMemberIndexAlias).Select(d => d.Id), Is.EqualTo(new[] { member.Key }));
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.DraftContent), Is.Empty);
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.Media), Is.Empty);
        });

        await ExternalMemberService.DeleteAsync(member.Key);

        Assert.Multiple(() =>
        {
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Is.Empty);
            Assert.That(IndexerAndSearcher.Dump(PublishedMemberIndexAlias), Is.Empty);
        });
    }

    [Test]
    public async Task ExternalMemberChanges_FromOtherOrigin_SkipSameOriginOnlyIndexes()
    {
        ExternalMemberIdentity member = await CreateExternalMemberAsync("external-seven@local", "External Seven");
        IndexerAndSearcher.Reset();

        GetRequiredService<ExternalMemberIndexingService>().Handle([member.Key], [], "some-other-origin");

        Assert.Multiple(() =>
        {
            Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member).Select(d => d.Id), Is.EqualTo(new[] { member.Key }));
            Assert.That(IndexerAndSearcher.Dump(SameOriginOnlyMemberIndexAlias), Is.Empty);
        });
    }

    [TestCase(IndexAliases.Member)]
    [TestCase(PublishedMemberIndexAlias)]
    public async Task RebuildMemberIndex_YieldsContentAndExternalMembers(string indexAlias)
    {
        IMemberType memberType = new MemberTypeBuilder().WithAlias("contentMemberType").Build();
        await GetRequiredService<IMemberTypeService>().CreateAsync(memberType, Constants.Security.SuperUserKey);

        var contentMemberKey = Guid.NewGuid();
        MemberService.Save(
            new MemberBuilder()
                .WithKey(contentMemberKey)
                .WithMemberType(memberType)
                .WithName("Content Member")
                .WithEmail("content-member@local")
                .WithLogin("content-member@local", "Test123456")
                .Build());

        ExternalMemberIdentity externalMember = await CreateExternalMemberAsync("external-eight@local", "External Eight");

        ContentIndexingService.Rebuild(indexAlias, DefaultOrigin);

        Assert.That(
            IndexerAndSearcher.Dump(indexAlias).Select(d => d.Id),
            Is.EquivalentTo(new[] { contentMemberKey, externalMember.Key }));
    }

    [Test]
    public async Task RebuildNonMemberIndex_DoesNotYieldExternalMembers()
    {
        await CreateExternalMemberAsync("external-nine@local", "External Nine");

        ContentIndexingService.Rebuild(IndexAliases.Media, DefaultOrigin);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Media), Is.Empty);
    }

    [Test]
    public async Task SearchMemberKeys_YieldsContentAndExternalMembers()
    {
        IMemberType memberType = new MemberTypeBuilder().WithAlias("contentMemberType").Build();
        await GetRequiredService<IMemberTypeService>().CreateAsync(memberType, Constants.Security.SuperUserKey);

        var contentMemberKey = Guid.NewGuid();
        MemberService.Save(
            new MemberBuilder()
                .WithKey(contentMemberKey)
                .WithMemberType(memberType)
                .WithName("Findable Content Member")
                .WithEmail("content-member@local")
                .WithLogin("content-member@local", "Test123456")
                .Build());

        ExternalMemberIdentity externalMember = await CreateExternalMemberAsync("external-ten@local", "Findable External Member");
        await CreateExternalMemberAsync("external-eleven@local", "Someone Else");

        IIndexedEntitySearchService indexedEntitySearchService = GetRequiredService<IIndexedEntitySearchService>();

        PagedModel<Guid> result = await indexedEntitySearchService.SearchKeysAsync(UmbracoObjectTypes.Member, "Findable", null, null, false);
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Is.EquivalentTo(new[] { contentMemberKey, externalMember.Key }));
            Assert.That(result.Total, Is.EqualTo(2));
        });

        // external members have no member type, so restricting the search to member types excludes them
        result = await indexedEntitySearchService.SearchKeysAsync(UmbracoObjectTypes.Member, "Findable", null, [memberType.Key], false);
        Assert.That(result.Items, Is.EqualTo(new[] { contentMemberKey }));
    }

    private async Task<ExternalMemberIdentity> CreateExternalMemberAsync(string email, string name)
    {
        var member = new ExternalMemberIdentity
        {
            Email = email,
            UserName = email,
            Name = name,
        };

        Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus> result = await ExternalMemberService.CreateAsync(member);
        Assert.That(result.Success, Is.True, $"Failed to create external member: {result.Status}");
        return result.Result;
    }

    private static IndexField? Field(TestIndexDocument document, string fieldName)
        => document.Fields.FirstOrDefault(field => field.FieldName == fieldName);

    private static string? Keyword(TestIndexDocument document, string fieldName)
        => Field(document, fieldName)?.Value.Keywords?.SingleOrDefault();

    private static int? Integer(TestIndexDocument document, string fieldName)
        => Field(document, fieldName)?.Value.Integers?.SingleOrDefault();
}
