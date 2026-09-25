using System.Security.Claims;
using Examine;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

[TestFixture]
internal class MemberIndexAuthorizerTests
{
    private const string MemberIndexName = "MembersIndex";
    private const string MemberSearcherName = "MembersSearcher";
    private const string ContentIndexName = "ExternalIndex";
    private const string ContentSearcherName = "ExternalSearcher";

    private Mock<IAuthorizationService> _authorizationServiceMock = null!;
    private MemberIndexAuthorizer _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var examineManagerMock = new Mock<IExamineManager>();
        examineManagerMock
            .SetupGet(x => x.Indexes)
            .Returns([
                CreateIndex<IUmbracoMemberIndex>(MemberIndexName, MemberSearcherName),
                CreateIndex<IUmbracoContentIndex>(ContentIndexName, ContentSearcherName)
            ]);

        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _sut = new MemberIndexAuthorizer(examineManagerMock.Object, _authorizationServiceMock.Object);
    }

    [Test]
    public void Can_Identify_Member_Index_By_Index_Name()
        => Assert.IsTrue(_sut.IsMemberIndex(MemberIndexName));

    [Test]
    public void Can_Identify_Member_Index_By_Searcher_Name()
        => Assert.IsTrue(_sut.IsMemberIndex(MemberSearcherName));

    [Test]
    public void Can_Identify_Member_Index_Ignoring_Case()
        => Assert.IsTrue(_sut.IsMemberIndex(MemberIndexName.ToLowerInvariant()));

    [Test]
    public void Cannot_Identify_Content_Index_As_Member_Index()
    {
        Assert.Multiple(() =>
        {
            Assert.IsFalse(_sut.IsMemberIndex(ContentIndexName));
            Assert.IsFalse(_sut.IsMemberIndex(ContentSearcherName));
        });
    }

    [Test]
    public void Cannot_Identify_Unknown_Name_As_Member_Index()
        => Assert.IsFalse(_sut.IsMemberIndex("NoSuchIndex"));

    [Test]
    public async Task Can_Access_Member_Data_When_Members_Section_Allowed()
    {
        SetupSectionAccess(allowed: true);

        Assert.IsTrue(await _sut.HasAccessAsync(new ClaimsPrincipal()));
    }

    [Test]
    public async Task Cannot_Access_Member_Data_When_Members_Section_Denied()
    {
        SetupSectionAccess(allowed: false);

        Assert.IsFalse(await _sut.HasAccessAsync(new ClaimsPrincipal()));
    }

    private void SetupSectionAccess(bool allowed)
        => _authorizationServiceMock
            .Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object?>(),
                AuthorizationPolicies.SectionAccessMembers))
            .ReturnsAsync(allowed ? AuthorizationResult.Success() : AuthorizationResult.Failed());

    private static IIndex CreateIndex<TIndex>(string indexName, string searcherName)
        where TIndex : class, IIndex
    {
        var searcherMock = new Mock<ISearcher>();
        searcherMock.SetupGet(x => x.Name).Returns(searcherName);

        var indexMock = new Mock<TIndex>();
        indexMock.SetupGet(x => x.Name).Returns(indexName);
        indexMock.SetupGet(x => x.Searcher).Returns(searcherMock.Object);

        return indexMock.Object;
    }
}
