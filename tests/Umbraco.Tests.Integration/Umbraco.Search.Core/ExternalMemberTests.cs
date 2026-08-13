using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Helpers;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;
using SearchConstants = Umbraco.Cms.Search.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class ExternalMemberTests : ContentBaseTestBase
{
    private IExternalMemberService ExternalMemberService => GetRequiredService<IExternalMemberService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IContentIndexingService ContentIndexingService => GetRequiredService<IContentIndexingService>();

    private IDateTimeOffsetConverter DateTimeOffsetConverter => GetRequiredService<IDateTimeOffsetConverter>();

    [SetUp]
    public void SetupTest() => IndexerAndSearcher.Reset();

    [Test]
    public async Task CreateExternalMember_IndexesDocument()
    {
        ExternalMemberIdentity member = await CreateExternalMemberAsync("external-one@local", "External One");

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(1));

        TestIndexDocument document = documents[0];
        Assert.Multiple(() =>
        {
            Assert.That(document.Id, Is.EqualTo(member.Key));
            Assert.That(document.ObjectType, Is.EqualTo(UmbracoObjectTypes.Member));

            var idValue = document.Fields.FirstOrDefault(f => f.FieldName == SearchConstants.FieldNames.Id)?.Value.Keywords?.SingleOrDefault();
            Assert.That(idValue, Is.EqualTo(member.Key.AsKeyword()));

            var nameValue = document.Fields.FirstOrDefault(f => f.FieldName == SearchConstants.FieldNames.Name)?.Value.TextsR1?.SingleOrDefault();
            Assert.That(nameValue, Is.EqualTo("External One"));

            var emailValue = document.Fields.FirstOrDefault(f => f.FieldName == SearchConstants.MemberFieldNames.Email)?.Value.Keywords?.SingleOrDefault();
            Assert.That(emailValue, Is.EqualTo("external-one@local"));

            var userNameValue = document.Fields.FirstOrDefault(f => f.FieldName == SearchConstants.MemberFieldNames.UserName)?.Value.Keywords?.SingleOrDefault();
            Assert.That(userNameValue, Is.EqualTo(member.UserName));

            var isExternalMemberValue = document.Fields.FirstOrDefault(f => f.FieldName == SearchConstants.MemberFieldNames.IsExternalMember)?.Value.Keywords?.SingleOrDefault();
            Assert.That(isExternalMemberValue, Is.EqualTo("1"));

            var isApprovedValue = document.Fields.FirstOrDefault(f => f.FieldName == SearchConstants.MemberFieldNames.IsApproved)?.Value.Integers?.SingleOrDefault();
            Assert.That(isApprovedValue, Is.EqualTo(1));

            DateTimeOffset? createDateValue = document.Fields.FirstOrDefault(f => f.FieldName == SearchConstants.FieldNames.CreateDate)?.Value.DateTimeOffsets?.SingleOrDefault();
            Assert.That(createDateValue, Is.EqualTo(DateTimeOffsetConverter.ToDateTimeOffset(member.CreateDate)));
        });
    }

    [Test]
    public async Task ContentMemberAndExternalMember_AreBothIndexedTogether()
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

        ExternalMemberIdentity externalMember = await CreateExternalMemberAsync("external-two@local", "External Two");

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(2));

        TestIndexDocument contentDocument = documents.Single(d => d.Id == contentMemberKey);
        TestIndexDocument externalDocument = documents.Single(d => d.Id == externalMember.Key);

        Assert.Multiple(() =>
        {
            Assert.That(contentDocument.Fields.Any(f => f.FieldName == SearchConstants.MemberFieldNames.IsExternalMember), Is.False);
            Assert.That(externalDocument.Fields.Any(f => f.FieldName == SearchConstants.MemberFieldNames.IsExternalMember), Is.True);
            Assert.That(contentDocument.ObjectType, Is.EqualTo(UmbracoObjectTypes.Member));
            Assert.That(externalDocument.ObjectType, Is.EqualTo(UmbracoObjectTypes.Member));
        });
    }

    [Test]
    public async Task DeleteExternalMember_RemovesDocument()
    {
        ExternalMemberIdentity member = await CreateExternalMemberAsync("external-three@local", "External Three");
        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Has.Count.EqualTo(1));

        await ExternalMemberService.DeleteAsync(member.Key);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Is.Empty);
    }

    [Test]
    public async Task UpdateLoginPropertiesAsync_SkipsReindex()
    {
        ExternalMemberIdentity member = await CreateExternalMemberAsync("external-four@local", "External Four");

        IndexerAndSearcher.Reset();

        member.LastLoginDate = DateTime.UtcNow;
        member.SecurityStamp = Guid.NewGuid().ToString();
        await ExternalMemberService.UpdateLoginPropertiesAsync(member);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Is.Empty, "Login-only updates should not trigger re-indexing.");
    }

    [Test]
    public async Task UpdateAsync_TriggersReindex()
    {
        ExternalMemberIdentity member = await CreateExternalMemberAsync("external-five@local", "External Five");

        IndexerAndSearcher.Reset();

        member.Name = "External Five Renamed";
        await ExternalMemberService.UpdateAsync(member);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents, Has.Count.EqualTo(1));

        var nameValue = documents[0].Fields.FirstOrDefault(f => f.FieldName == SearchConstants.FieldNames.Name)?.Value.TextsR1?.SingleOrDefault();
        Assert.That(nameValue, Is.EqualTo("External Five Renamed"));
    }

    [Test]
    public async Task RebuildIndex_YieldsExternalMembers()
    {
        ExternalMemberIdentity member = await CreateExternalMemberAsync("external-six@local", "External Six");

        IndexerAndSearcher.Reset();

        ContentIndexingService.Rebuild(IndexAliases.Member, DefaultOrigin);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Member);
        Assert.That(documents.Select(d => d.Id), Is.EquivalentTo(new[] { member.Key }));
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
}
