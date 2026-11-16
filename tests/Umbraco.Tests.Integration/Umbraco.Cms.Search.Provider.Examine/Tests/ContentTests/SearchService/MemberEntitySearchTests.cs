using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.SearchService;

/// <summary>
/// Verifies that backoffice member search matches identifying member data (email, username, property values and
/// external profile data) only for users with access to the Members section, and the member name for everyone else.
/// </summary>
[LongRunning]
public class MemberEntitySearchTests : SearcherTestBase
{
    private const string ContentMemberEmail = "cyan.alpha@quasar.test";
    private const string ContentMemberUserName = "orbitlogin";
    private const string ContentMemberOrganization = "Quokka";
    private const string ExternalMemberEmail = "teal.beta@pulsar.test";
    private const string ExternalMemberUserName = "nebulalogin";
    private const string ExternalMemberPetName = "Biscuit";

    private IUser? _currentUser;

    private Guid _contentMemberKey;
    private Guid _externalMemberKey;

    public enum CurrentUserKind
    {
        None,
        WithoutMembersAccess,
        WithMembersAccess,
    }

    private IIndexedEntitySearchService IndexedEntitySearchService => GetRequiredService<IIndexedEntitySearchService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        var backOfficeSecurity = new Mock<IBackOfficeSecurity>();
        backOfficeSecurity.SetupGet(security => security.CurrentUser).Returns(() => _currentUser);

        var backOfficeSecurityAccessor = new Mock<IBackOfficeSecurityAccessor>();
        backOfficeSecurityAccessor.SetupGet(accessor => accessor.BackOfficeSecurity).Returns(backOfficeSecurity.Object);

        builder.Services.AddUnique(backOfficeSecurityAccessor.Object);
    }

    [SetUp]
    public async Task SetupTest()
    {
        _currentUser = null;

        IMemberType memberType = new MemberTypeBuilder()
            .WithAlias("searchableMemberType")
            .AddPropertyGroup()
                .AddPropertyType()
                    .WithAlias("organization")
                    .WithDataTypeId(Constants.DataTypes.Textbox)
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                    .Done()
                .Done()
            .Build();
        await GetRequiredService<IMemberTypeService>().CreateAsync(memberType, Constants.Security.SuperUserKey);

        await WaitForIndexing(Constants.Search.IndexAliases.DraftMembers, async () =>
        {
            IMember member = new MemberBuilder()
                .WithMemberType(memberType)
                .WithName("Findable Content Member")
                .WithEmail(ContentMemberEmail)
                .WithLogin(ContentMemberUserName, "Test123456")
                .AddPropertyData()
                    .WithKeyValue("organization", ContentMemberOrganization)
                    .Done()
                .Build();
            GetRequiredService<IMemberService>().Save(member);
            _contentMemberKey = member.Key;

            Attempt<ExternalMemberIdentity, ExternalMemberOperationStatus> result = await GetRequiredService<IExternalMemberService>().CreateAsync(
                new ExternalMemberIdentity
                {
                    Email = ExternalMemberEmail,
                    UserName = ExternalMemberUserName,
                    Name = "Findable External Member",
                    ProfileData = $$"""{"petName":"{{ExternalMemberPetName}}"}""",
                });
            Assert.That(result.Success, Is.True);
            _externalMemberKey = result.Result.Key;
        });
    }

    [TestCase(CurrentUserKind.None, ContentMemberEmail)]
    [TestCase(CurrentUserKind.None, ContentMemberUserName)]
    [TestCase(CurrentUserKind.None, ContentMemberOrganization)]
    [TestCase(CurrentUserKind.None, ExternalMemberEmail)]
    [TestCase(CurrentUserKind.None, ExternalMemberUserName)]
    [TestCase(CurrentUserKind.None, ExternalMemberPetName)]
    [TestCase(CurrentUserKind.WithoutMembersAccess, ContentMemberEmail)]
    [TestCase(CurrentUserKind.WithoutMembersAccess, ContentMemberUserName)]
    [TestCase(CurrentUserKind.WithoutMembersAccess, ContentMemberOrganization)]
    [TestCase(CurrentUserKind.WithoutMembersAccess, ExternalMemberEmail)]
    [TestCase(CurrentUserKind.WithoutMembersAccess, ExternalMemberUserName)]
    [TestCase(CurrentUserKind.WithoutMembersAccess, ExternalMemberPetName)]
    public async Task Cannot_Match_Identifying_Member_Data_Without_Members_Access(CurrentUserKind currentUserKind, string query)
    {
        _currentUser = CreateUser(currentUserKind);

        PagedModel<Guid> result = await SearchAsync(query);

        Assert.That(result.Items, Is.Empty);
    }

    [TestCase(ContentMemberEmail, true)]
    [TestCase(ContentMemberUserName, true)]
    [TestCase(ContentMemberOrganization, true)]
    [TestCase(ExternalMemberEmail, false)]
    [TestCase(ExternalMemberUserName, false)]
    [TestCase(ExternalMemberPetName, false)]
    public async Task Can_Match_Identifying_Member_Data_With_Members_Access(string query, bool expectContentMember)
    {
        _currentUser = CreateUser(CurrentUserKind.WithMembersAccess);

        PagedModel<Guid> result = await SearchAsync(query);

        Assert.That(result.Items, Is.EqualTo(new[] { expectContentMember ? _contentMemberKey : _externalMemberKey }));
    }

    [TestCase(CurrentUserKind.None, "Findable")]
    [TestCase(CurrentUserKind.WithoutMembersAccess, "Findable")]
    [TestCase(CurrentUserKind.WithMembersAccess, "Findable")]
    [TestCase(CurrentUserKind.None, "*Findable ?")]
    [TestCase(CurrentUserKind.WithoutMembersAccess, "*Findable ?")]
    [TestCase(CurrentUserKind.WithMembersAccess, "*Findable ?")]
    public async Task Can_Match_Member_Name(CurrentUserKind currentUserKind, string query)
    {
        _currentUser = CreateUser(currentUserKind);

        PagedModel<Guid> result = await SearchAsync(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Is.EquivalentTo(new[] { _contentMemberKey, _externalMemberKey }));
            Assert.That(result.Total, Is.EqualTo(2));
        });
    }

    [TestCase(CurrentUserKind.None)]
    [TestCase(CurrentUserKind.WithoutMembersAccess)]
    [TestCase(CurrentUserKind.WithMembersAccess)]
    public async Task Can_Match_Member_Name_By_Every_Term(CurrentUserKind currentUserKind)
    {
        _currentUser = CreateUser(currentUserKind);

        PagedModel<Guid> result = await SearchAsync("Findable Exter");

        Assert.That(result.Items, Is.EqualTo(new[] { _externalMemberKey }));
    }

    [TestCase(CurrentUserKind.None)]
    [TestCase(CurrentUserKind.WithoutMembersAccess)]
    [TestCase(CurrentUserKind.WithMembersAccess)]
    public async Task Can_Match_Member_Key(CurrentUserKind currentUserKind)
    {
        _currentUser = CreateUser(currentUserKind);

        PagedModel<Guid> result = await SearchAsync(_contentMemberKey.ToString());

        Assert.That(result.Items, Is.EqualTo(new[] { _contentMemberKey }));
    }

    private Task<PagedModel<Guid>> SearchAsync(string query)
        => IndexedEntitySearchService.SearchKeysAsync(UmbracoObjectTypes.Member, query, null, null, null);

    private static IUser? CreateUser(CurrentUserKind currentUserKind)
    {
        if (currentUserKind is CurrentUserKind.None)
        {
            return null;
        }

        string[] sections = currentUserKind is CurrentUserKind.WithMembersAccess
            ? [Constants.Applications.Content, Constants.Applications.Members]
            : [Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Settings];

        var user = new Mock<IUser>();
        user.SetupGet(u => u.AllowedSections).Returns(sections);
        return user.Object;
    }
}
