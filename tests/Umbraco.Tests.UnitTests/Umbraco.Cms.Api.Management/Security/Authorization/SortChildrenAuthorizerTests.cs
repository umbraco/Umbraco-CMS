using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Security.Authorization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Security.Authorization;

[TestFixture]
public class SortChildrenAuthorizerTests
{
    private const string Policy = "TestPolicy";

    private Mock<IAuthorizationService> _authorizationService = null!;
    private Mock<IEntityService> _entityService = null!;
    private ClaimsPrincipal _user = null!;

    [SetUp]
    public void SetUp()
    {
        _authorizationService = new Mock<IAuthorizationService>();
        _entityService = new Mock<IEntityService>();
        _user = new ClaimsPrincipal(new ClaimsIdentity());
    }

    [Test]
    public async Task Returns_True_When_All_Children_Are_Authorized()
    {
        SetupChildren(skip: 0, total: 2, Guid.NewGuid(), Guid.NewGuid());
        SetupAuthorization(AuthorizationResult.Success());

        var result = await Authorize();

        Assert.IsTrue(result);
    }

    [Test]
    public async Task Returns_False_When_A_Child_Is_Not_Authorized()
    {
        // A single denied child must fail the whole operation - the controller turns this into a 403.
        SetupChildren(skip: 0, total: 2, Guid.NewGuid(), Guid.NewGuid());
        SetupAuthorization(AuthorizationResult.Failed());

        var result = await Authorize();

        Assert.IsFalse(result);
    }

    [Test]
    public async Task Returns_True_And_Does_Not_Authorize_When_There_Are_No_Children()
    {
        SetupChildren(skip: 0, total: 0);

        var result = await Authorize();

        Assert.IsTrue(result);
        _authorizationService.Verify(
            x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task Authorizes_Every_Page_Of_Children()
    {
        // 600 children across two pages of 500; both pages must be authorized.
        var firstPage = Enumerable.Range(0, 500).Select(_ => Guid.NewGuid()).ToArray();
        var secondPage = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToArray();
        SetupChildren(skip: 0, total: 600, firstPage);
        SetupChildren(skip: 500, total: 600, secondPage);
        SetupAuthorization(AuthorizationResult.Success());

        var result = await Authorize();

        Assert.IsTrue(result);
        _authorizationService.Verify(
            x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), Policy),
            Times.Exactly(2));
    }

    private Task<bool> Authorize() =>
        SortChildrenAuthorizer.IsAuthorizedForChildrenAsync(
            _authorizationService.Object,
            _entityService.Object,
            _user,
            parentKey: Guid.NewGuid(),
            UmbracoObjectTypes.Document,
            childKeys => ContentPermissionResource.WithKeys("A", childKeys),
            Policy);

    private void SetupChildren(int skip, long total, params Guid[] childKeys)
    {
        IEntitySlim[] children = childKeys
            .Select(key =>
            {
                var entity = new Mock<IEntitySlim>();
                entity.SetupGet(x => x.Key).Returns(key);
                return entity.Object;
            })
            .ToArray();

        var totalRecords = total;
        _entityService
            .Setup(x => x.GetPagedChildren(
                It.IsAny<Guid?>(),
                It.IsAny<IEnumerable<UmbracoObjectTypes>>(),
                It.IsAny<UmbracoObjectTypes>(),
                skip,
                It.IsAny<int>(),
                out totalRecords,
                It.IsAny<IQuery<IUmbracoEntity>>(),
                It.IsAny<Ordering>()))
            .Returns(children);
    }

    private void SetupAuthorization(AuthorizationResult result) =>
        _authorizationService
            .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(result);
}
