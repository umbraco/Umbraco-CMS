using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Persistence;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

/// <summary>
/// Verifies that changing whether an existing member type property is sensitive re-indexes the members of that type,
/// including their persisted change-detection snapshots.
/// </summary>
public class MemberTypeSensitivePropertyIndexingTests : ContentBaseTestBase
{
    private const string MemberTypeAlias = "myMemberType";
    private const string PropertyAlias = "organization";

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IContentIndexingService ContentIndexingService => GetRequiredService<IContentIndexingService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        // TestBase registers a no-op IIndexDocumentRepository, which would hide stale snapshots being replayed - use
        // the real, database-backed repository instead.
        builder.Services.AddUnique<IIndexDocumentRepository, IndexDocumentRepository>();
    }

    [Test]
    public async Task Existing_Property_Made_Sensitive_Is_Removed_From_Index()
    {
        await CreateMemberTypeWithMembers(isSensitive: false);
        Assert.That(IndexedValues(), Is.EquivalentTo(new[] { "Organization One", "Organization Two" }));

        await SetSensitive(true);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Has.Count.EqualTo(2));
        Assert.That(IndexedValues(), Is.Empty);
    }

    [Test]
    public async Task Existing_Property_Made_Sensitive_Stays_Removed_After_Rebuild()
    {
        await CreateMemberTypeWithMembers(isSensitive: false);

        await SetSensitive(true);
        ContentIndexingService.Rebuild(IndexAliases.Member, DefaultOrigin);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Has.Count.EqualTo(2));
        Assert.That(IndexedValues(), Is.Empty);
    }

    [Test]
    public async Task Sensitive_Property_Made_Non_Sensitive_Is_Indexed()
    {
        await CreateMemberTypeWithMembers(isSensitive: true);
        Assert.That(IndexedValues(), Is.Empty);

        await SetSensitive(false);

        Assert.That(IndexedValues(), Is.EquivalentTo(new[] { "Organization One", "Organization Two" }));
    }

    [Test]
    public async Task Member_Type_Save_Without_Sensitive_Change_Does_Not_Reindex_Members()
    {
        await CreateMemberTypeWithMembers(isSensitive: false);
        IndexerAndSearcher.Reset();

        IMemberType memberType = GetMemberType();
        memberType.Name = "Renamed member type";
        await MemberTypeService.UpdateAsync(memberType, Constants.Security.SuperUserKey);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Is.Empty);
    }

    [Test]
    public async Task New_Property_Added_As_Sensitive_Does_Not_Reindex_Members()
    {
        await CreateMemberTypeWithMembers(isSensitive: false);
        IndexerAndSearcher.Reset();

        IMemberType memberType = GetMemberType();
        memberType.AddPropertyType(
            new PropertyTypeBuilder()
                .WithAlias("secret")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .Build(),
            "group");
        memberType.SetIsSensitiveProperty("secret", true);
        await MemberTypeService.UpdateAsync(memberType, Constants.Security.SuperUserKey);

        Assert.That(IndexerAndSearcher.Dump(IndexAliases.Member), Is.Empty);
    }

    private async Task CreateMemberTypeWithMembers(bool isSensitive)
    {
        IMemberType memberType = new MemberTypeBuilder()
            .WithAlias(MemberTypeAlias)
            .AddPropertyGroup()
            .WithAlias("group")
            .WithName("Group")
            .AddPropertyType()
            .WithAlias(PropertyAlias)
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Done()
            .Build();
        memberType.SetIsSensitiveProperty(PropertyAlias, isSensitive);
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        SaveMember(memberType, "one", "Organization One");
        SaveMember(memberType, "two", "Organization Two");
    }

    private void SaveMember(IMemberType memberType, string name, string organization)
        => MemberService.Save(
            new MemberBuilder()
                .WithMemberType(memberType)
                .WithName($"Member {name}")
                .WithEmail($"member{name}@local")
                .WithLogin($"member{name}@local", "Test123456")
                .AddPropertyData()
                .WithKeyValue(PropertyAlias, organization)
                .Done()
                .Build());

    private async Task SetSensitive(bool isSensitive)
    {
        IMemberType memberType = GetMemberType();
        memberType.SetIsSensitiveProperty(PropertyAlias, isSensitive);
        await MemberTypeService.UpdateAsync(memberType, Constants.Security.SuperUserKey);
    }

    private IMemberType GetMemberType()
        => MemberTypeService.Get(MemberTypeAlias) ?? throw new InvalidOperationException("The member type was not found.");

    private string[] IndexedValues()
        => IndexerAndSearcher.Dump(IndexAliases.Member)
            .SelectMany(document => document.Fields.Where(field => field.FieldName == PropertyAlias))
            .SelectMany(field => field.Value.Texts ?? [])
            .ToArray();
}
