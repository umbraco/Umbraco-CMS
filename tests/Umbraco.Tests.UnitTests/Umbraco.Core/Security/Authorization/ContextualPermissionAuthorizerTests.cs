using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Security.Authorization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Security.Authorization;

[TestFixture]
public class ContextualPermissionAuthorizerTests
{
    private const string FirstContext = "firstContext";
    private const string SecondContext = "secondContext";

    private const string FirstPermission = "firstPermission";
    private const string SecondPermission = "secondPermission";
    private const string ThirdPermission = "thirdPermission";

    [Test]
    public void IsDenied_True_NoGranularPermissions()
    {
        // arrange
        var user = BuildUser(new HashSet<IGranularPermission>());
        var resource = ContextualPermissionResource.WithPermission(FirstPermission, FirstContext);
        var sut = new ContextualPermissionAuthorizer();

        // act
        var isDenied = sut.IsDenied(user, resource);

        // assert
        Assert.IsTrue(isDenied);
    }

    [Test]
    public void IsDenied_True_IncorrectGranularPermission()
    {
        // arrange
        var user = BuildUser(new HashSet<IGranularPermission>
        {
            new UnknownTypeGranularPermission { Context = FirstContext, Permission = SecondPermission },
        });
        var resource = ContextualPermissionResource.WithPermission(FirstPermission, FirstContext);
        var sut = new ContextualPermissionAuthorizer();

        // act
        var isDenied = sut.IsDenied(user, resource);

        // assert
        Assert.IsTrue(isDenied);
    }

    [Test]
    public void IsDenied_True_IncorrectGranularPermissions()
    {
        // arrange
        var user = BuildUser(new HashSet<IGranularPermission>
        {
            new UnknownTypeGranularPermission { Context = FirstContext, Permission = ThirdPermission },
            new UnknownTypeGranularPermission { Context = SecondContext, Permission = FirstPermission },
        });
        var resource =
            ContextualPermissionResource.WithAnyPermissions([FirstPermission, SecondPermission], FirstContext);

        var sut = new ContextualPermissionAuthorizer();

        // act
        var isDenied = sut.IsDenied(user, resource);

        // assert
        Assert.IsTrue(isDenied);
    }

    [Test]
    public void IsDenied_True_NotEnoughGranularPermissions()
    {
        // arrange
        var user = BuildUser(new HashSet<IGranularPermission>
        {
            new UnknownTypeGranularPermission { Context = FirstContext, Permission = SecondPermission },
            new UnknownTypeGranularPermission { Context = FirstContext, Permission = ThirdPermission },
        });
        var resource =
            ContextualPermissionResource.WithAllPermissions([FirstPermission, SecondPermission], FirstContext);

        var sut = new ContextualPermissionAuthorizer();

        // act
        var isDenied = sut.IsDenied(user, resource);

        // assert
        Assert.IsTrue(isDenied);
    }

    [Test]
    public void IsDenied_True_HasGranularPermissionsButToSpecific()
    {
        // arrange
        var user = BuildUser(new HashSet<IGranularPermission>
        {
            new DocumentGranularPermission { Permission = FirstPermission, Key = Guid.NewGuid() },
        });
        var resource =
            ContextualPermissionResource.WithContextWidePermission(FirstPermission, DocumentGranularPermission.ContextType);

        var sut = new ContextualPermissionAuthorizer();

        // act
        var isDenied = sut.IsDenied(user, resource);

        // assert
        Assert.IsTrue(isDenied);
    }

    [Test]
    public void IsDenied_False_HasGranularPermission()
    {
        // arrange
        var user = BuildUser(new HashSet<IGranularPermission>
        {
            new UnknownTypeGranularPermission { Context = FirstContext, Permission = FirstPermission },
        });
        var resource = ContextualPermissionResource.WithPermission(FirstPermission, FirstContext);
        var sut = new ContextualPermissionAuthorizer();

        // act
        var isDenied = sut.IsDenied(user, resource);

        // assert
        Assert.IsFalse(isDenied);
    }

    [Test]
    public void IsDenied_False_HasAnyGranularPermissions()
    {
        // arrange
        var user = BuildUser(new HashSet<IGranularPermission>
        {
            new UnknownTypeGranularPermission { Context = FirstContext, Permission = SecondPermission },
        });
        var resource =
            ContextualPermissionResource.WithAnyPermissions([FirstPermission, SecondPermission], FirstContext);

        var sut = new ContextualPermissionAuthorizer();

        // act
        var isDenied = sut.IsDenied(user, resource);

        // assert
        Assert.IsFalse(isDenied);
    }

    [Test]
    public void IsDenied_False_HasMoreThanGranularPermissions()
    {
        // arrange
        var user = BuildUser(new HashSet<IGranularPermission>
        {
            new UnknownTypeGranularPermission { Context = FirstContext, Permission = FirstPermission },
            new UnknownTypeGranularPermission { Context = FirstContext, Permission = SecondPermission },
        });
        var resource =
            ContextualPermissionResource.WithAllPermissions([FirstPermission, SecondPermission], FirstContext);

        var sut = new ContextualPermissionAuthorizer();

        // act
        var isDenied = sut.IsDenied(user, resource);

        // assert
        Assert.IsFalse(isDenied);
    }

    [Test]
    public void IsDenied_False_HasSpecificGranularPermissionForAnyMatch()
    {
        // arrange
        var user = BuildUser(new HashSet<IGranularPermission>
        {
            new DocumentGranularPermission { Permission = FirstPermission, Key = Guid.NewGuid() },
        });
        var resource =
            ContextualPermissionResource.WithPermission(FirstPermission, DocumentGranularPermission.ContextType);

        var sut = new ContextualPermissionAuthorizer();

        // act
        var isDenied = sut.IsDenied(user, resource);

        // assert
        Assert.IsFalse(isDenied);
    }

    private IUser BuildUser(ISet<IGranularPermission> granularPermissions)
    {
        var userGroups = new List<IReadOnlyUserGroup>();
        var groupMock = new Mock<IReadOnlyUserGroup>();
        groupMock
            .SetupGet(group => group.GranularPermissions)
            .Returns(granularPermissions);

        userGroups.Add(groupMock.Object);

        var userMock = new Mock<IUser>();
        userMock
            .SetupGet(u => u.Groups)
            .Returns(userGroups);

        return userMock.Object;
    }
}
