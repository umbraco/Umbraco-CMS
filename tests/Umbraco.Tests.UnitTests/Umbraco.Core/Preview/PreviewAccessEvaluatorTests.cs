using System.Security.Claims;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Preview;

[TestFixture]
public class PreviewAccessEvaluatorTests
{
    private const int StartNodeId = 1000;
    private const string InsideStartNodePath = "-1,1000,1001";
    private const string OutsideStartNodePath = "-1,2000,2001";

    private readonly Guid _userKey = Guid.NewGuid();
    private readonly Dictionary<Guid, string> _elementPaths = [];

    [Test]
    public void Allows_Any_Document_Without_A_Preview_User()
    {
        PreviewAccessEvaluator evaluator = CreateEvaluator(user: null, out _);

        Assert.IsTrue(evaluator.CanPreviewDocument(Content(OutsideStartNodePath)));
    }

    [Test]
    public void Allows_Document_Inside_Start_Node()
    {
        PreviewAccessEvaluator evaluator = CreateEvaluator(CreateUser(), out _);

        Assert.IsTrue(evaluator.CanPreviewDocument(Content(InsideStartNodePath)));
    }

    [Test]
    public void Refuses_Document_Outside_Start_Node()
    {
        PreviewAccessEvaluator evaluator = CreateEvaluator(CreateUser(), out _);

        Assert.IsFalse(evaluator.CanPreviewDocument(Content(OutsideStartNodePath)));
    }

    [Test]
    public void Refuses_Document_Without_Browse_Permission()
    {
        PreviewAccessEvaluator evaluator = CreateEvaluator(CreateUser(), out _, permissions: [ActionUpdate.ActionLetter]);

        Assert.IsFalse(evaluator.CanPreviewDocument(Content(InsideStartNodePath)));
    }

    [Test]
    public void Refuses_Document_Without_Content_Section_Access()
    {
        PreviewAccessEvaluator evaluator = CreateEvaluator(CreateUser(sections: [Constants.Applications.Settings]), out _);

        Assert.IsFalse(evaluator.CanPreviewDocument(Content(InsideStartNodePath)));
    }

    [Test]
    public void Evaluates_Each_Path_Once_Per_Request()
    {
        PreviewAccessEvaluator evaluator = CreateEvaluator(CreateUser(), out Mock<IUserService> userService);

        evaluator.CanPreviewDocument(Content(InsideStartNodePath));
        evaluator.CanPreviewDocument(Content(InsideStartNodePath));

        userService.Verify(x => x.GetPermissionsForPath(It.IsAny<IUser>(), InsideStartNodePath), Times.Once);
    }

    [Test]
    public void Allows_Any_Element_Without_A_Preview_User()
    {
        PreviewAccessEvaluator evaluator = CreateEvaluator(user: null, out _);

        Assert.IsTrue(evaluator.CanPreviewElement(Element(OutsideStartNodePath)));
    }

    [Test]
    public void Allows_Element_Inside_Element_Start_Node()
    {
        PreviewAccessEvaluator evaluator = CreateEvaluator(CreateUser(), out _);

        Assert.IsTrue(evaluator.CanPreviewElement(Element(InsideStartNodePath)));
    }

    [Test]
    public void Refuses_Element_Outside_Element_Start_Node()
    {
        PreviewAccessEvaluator evaluator = CreateEvaluator(CreateUser(), out _);

        Assert.IsFalse(evaluator.CanPreviewElement(Element(OutsideStartNodePath)));
    }

    [Test]
    public void Refuses_Element_With_Only_Document_Browse_Permission()
    {
        PreviewAccessEvaluator evaluator = CreateEvaluator(CreateUser(), out _, permissions: [ActionBrowse.ActionLetter]);

        Assert.IsFalse(evaluator.CanPreviewElement(Element(InsideStartNodePath)));
    }

    [Test]
    public void Refuses_Element_Without_Library_Section_Access()
    {
        PreviewAccessEvaluator evaluator = CreateEvaluator(CreateUser(sections: [Constants.Applications.Content]), out _);

        Assert.IsFalse(evaluator.CanPreviewElement(Element(InsideStartNodePath)));
    }

    private IPublishedElement Element(string path)
    {
        var key = Guid.NewGuid();
        _elementPaths[key] = path;
        return Mock.Of<IPublishedElement>(x => x.Key == key);
    }

    private static IPublishedContent Content(string path)
        => Mock.Of<IPublishedContent>(x => x.Path == path && x.Key == Guid.NewGuid());

    private IUser CreateUser(string[]? sections = null)
    {
        IReadOnlyUserGroup[] groups = [Mock.Of<IReadOnlyUserGroup>(x => x.StartContentId == StartNodeId && x.StartElementId == StartNodeId)];

        var user = new Mock<IUser>();
        user.SetupGet(x => x.Key).Returns(_userKey);
        user.SetupGet(x => x.Id).Returns(1234);
        user.SetupGet(x => x.AllowedSections).Returns(sections ?? [Constants.Applications.Content, Constants.Applications.Library]);
        user.SetupGet(x => x.Groups).Returns(groups);
        user.SetupGet(x => x.StartContentIds).Returns([]);
        user.SetupGet(x => x.StartElementIds).Returns([]);
        return user.Object;
    }

    private PreviewAccessEvaluator CreateEvaluator(IUser? user, out Mock<IUserService> userService, string[]? permissions = null)
    {
        var previewService = new Mock<IPreviewService>();
        previewService
            .Setup(x => x.TryGetPreviewClaimsIdentityAsync())
            .ReturnsAsync(user is null
                ? Attempt<ClaimsIdentity>.Fail()
                : Attempt.Succeed(new ClaimsIdentity([new Claim(Constants.Security.OpenIdDictSubClaimType, _userKey.ToString())])));

        userService = new Mock<IUserService>();
        userService.Setup(x => x.GetAsync(_userKey)).ReturnsAsync(user);
        userService
            .Setup(x => x.GetPermissionsForPath(It.IsAny<IUser>(), It.IsAny<string>()))
            .Returns(new EntityPermissionSet(
                StartNodeId,
                [new EntityPermission(1, StartNodeId, new HashSet<string>(permissions ?? [ActionBrowse.ActionLetter, ActionElementBrowse.ActionLetter]))]));

        var entityService = new Mock<IEntityService>();
        entityService
            .Setup(x => x.GetAllPaths(UmbracoObjectTypes.Document, It.IsAny<int[]>()))
            .Returns((UmbracoObjectTypes _, int[] ids) => ids.Select(id => new TreeEntityPath { Id = id, Path = $"-1,{id}" }));
        entityService
            .Setup(x => x.GetAllPaths(UmbracoObjectTypes.ElementContainer, It.IsAny<int[]>()))
            .Returns((UmbracoObjectTypes _, int[] ids) => ids.Select(id => new TreeEntityPath { Id = id, Path = $"-1,{id}" }));
        entityService
            .Setup(x => x.GetAllPaths(UmbracoObjectTypes.Element, It.IsAny<Guid[]>()))
            .Returns((UmbracoObjectTypes _, Guid[] keys) => keys
                .Where(_elementPaths.ContainsKey)
                .Select(key => new TreeEntityPath { Id = 0, Path = _elementPaths[key] }));

        // A request cache is required for the per-request memoization to be observable.
        var appCaches = new AppCaches(NoAppCache.Instance, new DictionaryAppCache(), new IsolatedCaches(_ => NoAppCache.Instance));

        return new PreviewAccessEvaluator(previewService.Object, userService.Object, entityService.Object, appCaches);
    }
}
